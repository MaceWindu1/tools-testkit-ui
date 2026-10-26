import { MetadataRepository, RecordRepository } from '../repositories';
import { DeepInsertResponse } from './deepInsertResponse';
import Record from './record';

/**
 * Parses deep insert objects and returns references to all created records.
 *
 * @export
 * @class DeepInsertService
 */
export default class DeepInsertService {
  private readonly recordRepository: RecordRepository;

  private readonly metadataRepository: MetadataRepository;

  /**
     * Creates an instance of DeepInsertService.
     * @param {MetadataRepository} metadataRepository A metadata repository.
     * @param {RecordRepository} recordRepository A record repository.
     * @memberof DeepInsertService
     */
  constructor(metadataRepository: MetadataRepository, recordRepository: RecordRepository) {
    this.metadataRepository = metadataRepository;
    this.recordRepository = recordRepository;
  }

  /**
     * A deep insert which returns a reference to all created records.
     *
     * @param {string} logicalName The entity logical name of the root record.
     * @param {Record} record The deep insert object.
     * @param dataByAlias References to previously created records by alias.
     * @param {RecordRepository} repository An optional repository to override the default.
     * @returns {Promise<DeepInsertResponse>} An async result with references to created records.
     * @memberof DeepInsertService
     */
  public async deepInsert(
    logicalName: string,
    record: Record,
    dataByAlias: { [alias: string]: Xrm.LookupValue },
    repository?: RecordRepository,
  ): Promise<DeepInsertResponse> {
    const repo = repository ?? this.recordRepository;
    const recordToCreate = record;
    const associatedRecords: { alias?: string, reference: Xrm.LookupValue }[] = [];

    const aliasedRecordsByNavProp = DeepInsertService.getAliasedLookups(recordToCreate);
    await Promise.all(Object.keys(aliasedRecordsByNavProp).map(async (aliasedRecordNavProp) => {
      const alias = recordToCreate[aliasedRecordNavProp] as string;
      const reference = dataByAlias[alias];
      if (!reference) {
        throw new Error(`Unable to bind ${aliasedRecordNavProp} as a record with the alias '${alias}' has not been created.`);
      }
      const set = await this.metadataRepository
        .getEntitySetForEntity(dataByAlias[alias].entityType);
      delete recordToCreate[aliasedRecordNavProp];
      recordToCreate[aliasedRecordNavProp.replace('@alias.bind', '@odata.bind')] = `/${set}(${dataByAlias[alias].id})`;
    }));

    const lookupRecordsByNavProp = DeepInsertService.getManyToOneRecords(recordToCreate);
    const singleNavProps = Object.keys(lookupRecordsByNavProp);
    await Promise.all(singleNavProps.map(async (singleNavProp) => {
      const res = await this.createLookupRecord(
        logicalName, recordToCreate, lookupRecordsByNavProp, singleNavProp, dataByAlias, repo,
      );
      associatedRecords.push(res.record, ...res.associatedRecords);
    }));

    const collRecordsByNavProp = DeepInsertService.getOneToManyRecords(recordToCreate);
    Object.keys(collRecordsByNavProp).forEach((collNavProp) => delete recordToCreate[collNavProp]);

    // Dataverse creates records active: an inactive state is set once the record and its related records exist.
    const inactiveState = DeepInsertService.getInactiveState(recordToCreate);
    if (inactiveState) {
      Object.keys(inactiveState).forEach((attribute) => delete recordToCreate[attribute]);
    }

    const recordToCreateRef = await repo.upsertRecord(logicalName, recordToCreate);

    if (Object.keys(record).includes('@bpf')) {
        DeepInsertService.setBusinessProcessFlowStage(record, recordToCreateRef.id, repo);
    }

    await Promise.all(Object.keys(collRecordsByNavProp).map(async (collNavProp) => {
      const result = await this.createCollectionRecords(
        logicalName, recordToCreateRef, collRecordsByNavProp, collNavProp, dataByAlias, repo,
      );
      associatedRecords.push(...result);
    }));

    if (inactiveState) {
      await repo.updateRecord(logicalName, recordToCreateRef.id, inactiveState);
    } else if (recordToCreate.statuscode !== undefined) {
      await DeepInsertService.keepStatusReason(
        repo, logicalName, recordToCreateRef.id, recordToCreate.statuscode as number,
      );
    }

    return {
      associatedRecords,
      record: {
        alias: recordToCreate['@alias'] as string | undefined,
        reference: recordToCreateRef,
      },
    };
  }

  private static getAliasedLookups(record: Record) {
    return Object.keys(record)
      .filter((key) => key.indexOf('@alias.bind') > -1)
      .reduce((prev, curr) => {
        // eslint-disable-next-line no-param-reassign
        prev[curr] = record[curr] as Record[];
        return prev;
      }, {} as { [navigationProperty: string]: Record[] });
  }

  // The state of a record that is not active (statecode other than 0), with its status reason when given.
  private static getInactiveState(record: Record): Record | undefined {
    if (record.statecode === undefined || record.statecode === 0) {
      return undefined;
    }

    return record.statuscode === undefined
      ? { statecode: record.statecode }
      : { statecode: record.statecode, statuscode: record.statuscode };
  }

  // Plugins and process flows can give a record they create another status (e.g. a contract starts
  // as a draft): the status reason the data gives an active record is then set again, as the
  // record's later updates would set it.
  private static async keepStatusReason(
    repo: RecordRepository,
    logicalName: string,
    id: string,
    statuscode: number,
  ): Promise<void> {
    const created = await repo.retrieveRecord(logicalName, id, '?$select=statuscode');
    if (created?.statuscode !== statuscode) {
      await repo.updateRecord(logicalName, id, { statuscode });
    }
  }

  private static getOneToManyRecords(record: Record)
    : { [navigationProperty: string]: Record[] } {
    return Object.keys(record)
      .filter((key) => Array.isArray(record[key]))
      .reduce((prev, curr) => {
        // eslint-disable-next-line no-param-reassign
        prev[curr] = record[curr] as Record[];
        return prev;
      }, {} as { [navigationProperty: string]: Record[] });
  }

  private static getManyToOneRecords(record: Record)
    : { [navigationProperty: string]: Record } {
    return Object.keys(record)
      .filter(
        (key) => typeof record[key] === 'object'
        && !Array.isArray(record[key])
        && record[key] !== null
        && !(record[key] instanceof Date)
        && (key !== '@bpf'),
      )
      .reduce((prev, curr) => {
        // eslint-disable-next-line no-param-reassign
        prev[curr] = record[curr] as Record;
        return prev;
      }, {} as { [navigationProperty: string]: Record });
  }

  private async createLookupRecord(
    logicalName: string,
    entity: Record,
    navigationPropertyMap: { [navigationProperty: string]: Record },
    singleNavProp: string,
    createdRecordsByAlias: { [alias: string]: Xrm.LookupValue },
    repository: RecordRepository,
  ): Promise<DeepInsertResponse> {
    const record = entity;
    delete record[singleNavProp];

    let entityName: string | null = null;
    const lookupEntity = navigationPropertyMap[singleNavProp];

    const targets = await this.metadataRepository.getTargetsForLookupProperty(
      logicalName, singleNavProp,
    );

    if (!targets && (singleNavProp.endsWith('_account') || singleNavProp.endsWith('_contact'))) {
      // Possibly a customer field
      const fieldWithoutSuffix = singleNavProp.replace('_account', '').replace('_contact', '');
      const customerTargets = await this.metadataRepository.getTargetsForLookupProperty(
        logicalName, fieldWithoutSuffix,
      );
      if (customerTargets && customerTargets.length === 2) {
        entityName = singleNavProp.endsWith('account') ? 'account' : 'contact';
      }
    } else if (targets && targets.length === 1) {
      [entityName] = targets;
    } else if (lookupEntity['@logicalName']) {
      entityName = lookupEntity['@logicalName'] as string;
    }

    if (!entityName) {
      throw new Error(`Unable to determine target entity for ${singleNavProp}.`);
    }

    const deepInsertResponse = await this.deepInsert(
      entityName, navigationPropertyMap[singleNavProp], createdRecordsByAlias, repository,
    );
    const entitySet = await this.metadataRepository.getEntitySetForEntity(entityName);
    record[`${singleNavProp}@odata.bind`] = `/${entitySet}(${deepInsertResponse.record.reference.id})`;

    return deepInsertResponse;
  }

  private async createCollectionRecords(
    logicalName: string,
    parent: Xrm.LookupValue,
    navPropMap: { [navigationProperty: string]: Record[] },
    collNavProp: string,
    refsByAlias: { [alias: string]: Xrm.LookupValue },
    repository: RecordRepository,
  ): Promise<{ alias?: string, reference: Xrm.LookupValue }[]> {
    const relMetadata = await this.metadataRepository.getRelationshipMetadata(collNavProp);
    const set = await this.metadataRepository.getEntitySetForEntity(logicalName);

    if (DeepInsertService.isOneToManyMetadata(relMetadata)) {
      return this.createOneToManyRecords(
        relMetadata.ReferencingEntity,
        set,
        collNavProp,
        navPropMap,
        parent,
        refsByAlias,
        repository,
      );
    }

    const entity = relMetadata.Entity1LogicalName !== logicalName
      ? relMetadata.Entity1LogicalName : relMetadata.Entity2LogicalName;

    return this.createManyToManyRecords(
      entity,
      collNavProp,
      navPropMap,
      parent,
      refsByAlias,
      repository,
    );
  }

  private async createOneToManyRecords(
    entity: string,
    entitySet: string,
    navProp: string,
    navPropMap: { [navProp: string]: Record[] },
    parent: Xrm.LookupValue,
    refsByAlias: { [alias: string]: Xrm.LookupValue },
    repository: RecordRepository,
  ): Promise<{ alias?: string, reference: Xrm.LookupValue }[]> {
    const oppNavProp = await this.metadataRepository
      .getLookupPropertyForCollectionProperty(navProp);

    const res = await Promise.all(navPropMap[navProp].map((oneToManyRecord) => {
      // eslint-disable-next-line no-param-reassign
      oneToManyRecord[`${oppNavProp}@odata.bind`] = `/${entitySet}(${parent.id})`;
      return this.deepInsert(entity, oneToManyRecord, refsByAlias, repository);
    }));

    return res.reduce<{ reference: Xrm.LookupValue; alias?: string; }[]>(
      (prev, curr) => prev.concat([curr.record, ...curr.associatedRecords]), [],
    );
  }

  private async createManyToManyRecords(
    entity: string,
    navProp: string,
    navPropMap: { [navProp: string]: Record[] },
    parent: Xrm.LookupValue,
    createdRecordsByAlias: { [alias: string]: Xrm.LookupValue },
    repository: RecordRepository,
  ): Promise<{ alias?: string, reference: Xrm.LookupValue }[]> {
    const result = await Promise.all(navPropMap[navProp].map(async (manyToManyRecord) => {
      const response = await this.deepInsert(
        entity,
        manyToManyRecord,
        createdRecordsByAlias,
        repository,
      );

      await repository.associateManyToManyRecords(
        parent,
        [response.record.reference],
        navProp,
      );

      return [response.record, ...response.associatedRecords];
    }));

    return result.reduce((prev, curr) => prev.concat(curr));
  }

  private static isOneToManyMetadata(
    metadata: Xrm.Metadata.RelationshipMetadata,
  ): metadata is Xrm.Metadata.OneToNRelationshipMetadata {
    return metadata.RelationshipType === 'OneToManyRelationship';
  }

private static async setBusinessProcessFlowStage(
    record: Record,
    recordId: string,
    repo: RecordRepository,
) {
    const bpfValue = record?.['@bpf'] as any;
    const bpfKeys = Object.keys(bpfValue);
    if (bpfKeys.includes('@logicalName') && bpfKeys.includes('@activestageid')) {
        const bpfRecords = await repo.retrieveMultipleRecords(bpfValue?.['@logicalName'], `?$filter=_bpf_${record?.['@logicalName']}id_value eq ${recordId}&$select=businessprocessflowinstanceid`);
        if (bpfRecords.entities.length === 1) {
            const bpfEntityId = bpfRecords.entities[0]?.businessprocessflowinstanceid;
            const bpfRecord: Record = {
                'activestageid@odata.bind': `/processstages(${bpfValue['@activestageid']})`,
            };
            repo.updateRecord(bpfValue?.['@logicalName'], bpfEntityId, bpfRecord);
        }
    }
}
}
