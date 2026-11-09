import { MetadataRepository } from '../../src/repositories';

// The ES module build cannot be bundled by karma-typescript.
const fetchMock = require('fetch-mock/es5/client');

describe('MetadataRepository', () => {
  const apiUrl = 'api/data/v9.1/';

  let metadataRepo: MetadataRepository;

  // The URL of the last request, without its encoding.
  const requestedUrl = (): string => decodeURIComponent(fetchMock.lastUrl());

  beforeEach(() => {
    metadataRepo = new MetadataRepository();
  });

  afterEach(() => {
    fetchMock.restore();
  });

  describe('getEntitySetForEntity(logicalName)', () => {
    it('returns the entity set from the entity metadata', async () => {
      const mockResponse = { value: [{ EntitySetName: 'contacts' }] };
      fetchMock.mock('*', { body: mockResponse });

      await expectAsync(metadataRepo.getEntitySetForEntity('contact'))
        .toBeResolvedTo('contacts');

      expect(requestedUrl()).toContain(`${apiUrl}EntityDefinitions?$filter=LogicalName eq 'contact'&$select=EntitySetName`);
    });
  });

  describe('getTargetsForLookupProperty(logicalName, navigationProperty)', () => {
    const entityLogicalName = 'contact';
    const targetEntityName = 'account';

    it('returns the target entity for a lookup attribute', async () => {
      const mockResponse = { value: [{ Targets: [targetEntityName] }] };
      fetchMock.mock('*', { body: mockResponse });

      await expectAsync(metadataRepo.getTargetsForLookupProperty(entityLogicalName, 'ParentCustomerId'))
        .toBeResolvedTo(mockResponse.value[0].Targets);

      expect(requestedUrl()).toContain(
        `${apiUrl}EntityDefinitions(LogicalName='contact')/Attributes/Microsoft.Dynamics.CRM.LookupAttributeMetadata`
        + "?$filter=LogicalName eq 'parentcustomerid'&$select=Targets",
      );
    });

    it('returns null if the attribute is not found', async () => {
      const mockResponse = { value: [] };
      fetchMock.mock('*', { body: mockResponse });

      await expectAsync(metadataRepo.getTargetsForLookupProperty(entityLogicalName, 'accountid'))
        .toBeResolvedTo(null);
    });
  });

  describe('getLookupPropertyForCollectionProperty(navPropName)', () => {
    it('returns the lookup navigation property for a collection navigation property', async () => {
      const lookupNavProp = 'contactid';
      const mockResponse = { value: [{ ReferencingEntityNavigationPropertyName: lookupNavProp }] };
      fetchMock.mock('*', { body: mockResponse });

      await expectAsync(metadataRepo.getLookupPropertyForCollectionProperty('contact_accounts'))
        .toBeResolvedTo(lookupNavProp);

      expect(requestedUrl()).toContain(
        `${apiUrl}RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata`
        + "?$filter=ReferencedEntityNavigationPropertyName eq 'contact_accounts'&$select=ReferencingEntityNavigationPropertyName",
      );
    });
  });

  describe('getRelationshipMetadata(relationshipSchemaName)', () => {
    it('returns relationship metadata for the provided relationship schema name', async () => {
      const result = { SchemaName: 'contact_accounts' };
      fetchMock.mock('*', { body: result });

      await expectAsync(metadataRepo.getRelationshipMetadata('contact_accounts'))
        .toBeResolvedTo(result as never);

      expect(requestedUrl()).toContain(`${apiUrl}RelationshipDefinitions(SchemaName='contact_accounts')`);
    });
  });
});
