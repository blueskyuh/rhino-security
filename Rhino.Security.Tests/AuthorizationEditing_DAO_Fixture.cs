namespace Rhino.Security.Tests
{
    using System;
    using Commons;
    using MbUnit.Framework;

    [TestFixture]
    public class AuthorizationEditing_DAO_Fixture : DatabaseFixture
    {
        [Test]
        public void CanSaveUser()
        {
            User ayende = new User();
            ayende.Name = "ayende";
            UnitOfWork.CurrentSession.Save(ayende);
            UnitOfWork.CurrentSession.Flush();
            UnitOfWork.CurrentSession.Evict(ayende);

            User fromDb = UnitOfWork.CurrentSession.Get<User>(ayende.Id);
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(ayende.Name, fromDb.Name);
        }

        [Test]
        public void CanSaveAccount()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";
            Assert.AreNotEqual(Guid.Empty, ayende.SecurityKey);
            UnitOfWork.CurrentSession.Save(ayende);
            UnitOfWork.CurrentSession.Flush();
            UnitOfWork.CurrentSession.Evict(ayende);

            Account fromDb = UnitOfWork.CurrentSession.Get<Account>(ayende.Id);
            Assert.IsNotNull(fromDb);
            Assert.AreEqual(ayende.Name, fromDb.Name);
            Assert.AreEqual(fromDb.SecurityKey, ayende.SecurityKey);
        }

        [Test]
        public void CanCreateUsersGroup()
        {
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admininstrators");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup groupFromDb = Repository<UsersGroup>.Get(group.Id);
            Assert.IsNotNull(groupFromDb);
            Assert.AreEqual(group.Name, groupFromDb.Name);
        }

        [Test]
        public void CanCreateEntitesGroup()
        {
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            EntitiesGroup groupFromDb = Repository<EntitiesGroup>.Get(group.Id);
            Assert.IsNotNull(groupFromDb);
            Assert.AreEqual(group.Name, groupFromDb.Name);
        }

        [Test]
        [ExpectedException(typeof (ValidationException))]
        public void CannotCreateEntitiesGroupWithSameName()
        {
            authorizationEditingService.CreateEntitiesGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateEntitiesGroup("Admininstrators");
        }

        [Test]
        [ExpectedException(typeof (ValidationException))]
        public void CannotCreateUsersGroupsWithSameName()
        {
            authorizationEditingService.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateUsersGroup("Admininstrators");
        }

        [Test]
        public void CanGetUsersGroupByName()
        {
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetUsersGroupByName("Admininstrators");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CanGetEntitiesGroupByName()
        {
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetEntitiesGroupByName("Accounts");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CannotChangeUsersGroupName()
        {
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admininstrators");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetUsersGroupByName("Admininstrators");
            group.Name = "2";
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetUsersGroupByName("2");
            Assert.IsNull(group);
            group = authorizationEditingService.GetUsersGroupByName("Admininstrators");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CannotChangeEntitiesGroupName()
        {
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetEntitiesGroupByName("Accounts");
            group.Name = "2";
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(group);

            group = authorizationEditingService.GetEntitiesGroupByName("2");
            Assert.IsNull(group);
            group = authorizationEditingService.GetEntitiesGroupByName("Accounts");
            Assert.IsNotNull(group);
        }

        [Test]
        public void CanAssociateUserWithGroup()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            UsersGroup group = authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(ayende, "Admins");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup[] groups = authorizationEditingService.GetAssociatedUsersGroupFor(ayende);
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
        }

        [Test]
        public void CanAssociateUserWithNestedGroup()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UsersGroup group = authorizationEditingService.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(ayende, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);

            UsersGroup[] groups = authorizationEditingService.GetAssociatedUsersGroupFor(ayende);
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
            Assert.AreEqual("DBA", groups[1].Name);
        }


        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWithNested()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(ayende, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("DBA", groups[0].Name);
            Assert.AreEqual("Admins", groups[1].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupDirect()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(ayende, "Admins");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereNonExists()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();


            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(0, groups.Length);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsDirectPathShouldSelectThat()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.AssociateUserWith(ayende, "Admins");
            authorizationEditingService.AssociateUserWith(ayende, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Admins", groups[0].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsTwoLevelNesting()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateChildUserGroupOf("DBA", "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.AssociateUserWith(ayende, "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(3, groups.Length);
            Assert.AreEqual("SQLite DBA", groups[0].Name);
            Assert.AreEqual("DBA", groups[1].Name);
            Assert.AreEqual("Admins", groups[2].Name);
        }

        [Test]
        public void CanGetAncestryAssociationOfUserWithGroupWhereThereIsMoreThanOneIndirectPathShouldSelectShortest()
        {
            User ayende = new User();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            authorizationEditingService.CreateUsersGroup("Admins");
            UnitOfWork.Current.TransactionalFlush();
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateChildUserGroupOf("Admins", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.CreateChildUserGroupOf("DBA", "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.AssociateUserWith(ayende, "DBA");
            authorizationEditingService.AssociateUserWith(ayende, "SQLite DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] groups = authorizationEditingService.GetAncestryAssociation(ayende, "Admins");
            Assert.AreEqual(2, groups.Length);
            Assert.AreEqual("DBA", groups[0].Name);
            Assert.AreEqual("Admins", groups[1].Name);
        }

        [Test]
        public void CanAssociateAccountWithGroup()
        {
            Account ayende = new Account();
            ayende.Name = "ayende";

            UnitOfWork.CurrentSession.Save(ayende);
            EntitiesGroup group = authorizationEditingService.CreateEntitiesGroup("Accounts");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateEntityWith(ayende, "Accounts");

            UnitOfWork.Current.TransactionalFlush();

            UnitOfWork.CurrentSession.Evict(ayende);
            UnitOfWork.CurrentSession.Evict(group);

            EntitiesGroup[] groups = authorizationEditingService.GetAssociatedEntitiesGroupsFor(ayende);
            Assert.AreEqual(1, groups.Length);
            Assert.AreEqual("Accounts", groups[0].Name);
        }

        [Test]
        public void CanCreateOperation()
        {
            authorizationEditingService.CreateOperation("/Account/Delete");
            UnitOfWork.Current.TransactionalFlush();
            Operation operation = authorizationEditingService.GetOperationByName("/Account/Delete");
            Assert.IsNotNull(operation, "Could not create operation");
        }

        [Test]
        public void WhenCreatingNestedOperation_WillCreateParentOperation_IfDoesNotExists()
        {
            Operation operation = authorizationEditingService.CreateOperation("/Account/Delete");
            UnitOfWork.Current.TransactionalFlush();
            Operation parentOperation = authorizationEditingService.GetOperationByName("/Account");
            Assert.IsNotNull(parentOperation);
            Assert.AreEqual(operation.Parent, parentOperation);
        }

        [Test]
        public void WhenCreatingNestedOperation_WillLinkToParentOperation()
        {
            authorizationEditingService.CreateOperation("/Account/Delete");
            UnitOfWork.Current.TransactionalFlush();
            Operation parentOperation = authorizationEditingService.GetOperationByName("/Account");
            Assert.IsNotNull(parentOperation); // was created in setup
            Assert.AreEqual(2, parentOperation.Children.Count); // /Edit, /Delete
        }

        [Test]
        public void CanRemoveUserGroup()
        {
            authorizationEditingService.RemoveUsersGroup("Administrators");
            UnitOfWork.Current.TransactionalFlush();

            Assert.IsNull(authorizationEditingService.GetUsersGroupByName("Administrators"));
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException),
            "Cannot remove users group 'Administrators' because is has child groups. Remove those groups and try again."
            )]
        public void RemovingParentUserGroupWillFail()
        {
            authorizationEditingService.CreateChildUserGroupOf("Administrators", "DBA");
            UnitOfWork.Current.TransactionalFlush();
            authorizationEditingService.RemoveUsersGroup("Administrators");
        }


        [Test]
        public void WhenRemovingUsersGroupThatHasAssociatedPermissionsThoseShouldBeRemoved()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For("Administrators")
                .OnEverything()
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();

            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.IsNotEmpty(permissions);

            authorizationEditingService.RemoveUsersGroup("Administrators");
            UnitOfWork.Current.TransactionalFlush();

            permissions = permissionService.GetPermissionsFor(user);
            Assert.IsEmpty(permissions);
        }

        [Test]
        public void CanRemoveNestedUserGroup()
        {
            UsersGroup dbaGroup = authorizationEditingService.CreateChildUserGroupOf("Administrators", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.RemoveUsersGroup("DBA");
            UnitOfWork.Current.TransactionalFlush();

            Assert.IsNull(authorizationEditingService.GetUsersGroupByName("DBA"));

            UsersGroup administratorsGroup = 
                authorizationEditingService.GetUsersGroupByName("Administrators");
            Assert.AreEqual(0,
                            administratorsGroup.DirectChildren.Count
                );
            Assert.AreEqual(0,
                            administratorsGroup.AllChildren.Count
                );

            Assert.AreEqual(0, dbaGroup.AllParents.Count);
        }

        [Test]
        public void UsersAreNotAssociatedWithRemovedGroups()
        {
            authorizationEditingService.CreateChildUserGroupOf("Administrators", "DBA");
            UnitOfWork.Current.TransactionalFlush();

            authorizationEditingService.AssociateUserWith(user, "DBA");
            UnitOfWork.Current.TransactionalFlush();

            UsersGroup[] associedGroups = authorizationEditingService.GetAssociatedUsersGroupFor(user);
            Assert.AreEqual(2, associedGroups.Length);

            authorizationEditingService.RemoveUsersGroup("DBA");
            UnitOfWork.Current.TransactionalFlush();


            associedGroups = authorizationEditingService.GetAssociatedUsersGroupFor(user);
            Assert.AreEqual(1, associedGroups.Length);
        }

        [Test]
        public void CanRemoveEntitiesGroup()
        {
            authorizationEditingService.RemoveEntitiesGroup("Important Accounts");
            UnitOfWork.Current.TransactionalFlush();
            Assert.IsNull(authorizationEditingService.GetEntitiesGroupByName("Important Accounts")); ;
        }


        [Test]
        public void WhenRemovingEntitiesGroupAllPermissionsOnItWillBeDeleted()
        {
            permissionsBuilderService
                .Allow("/Account/Edit")
                .For(user)
                .On("Important Accounts")
                .DefaultLevel()
                .Save();
            UnitOfWork.Current.TransactionalFlush();
       
            Permission[] permissions = permissionService.GetPermissionsFor(user);
            Assert.IsNotEmpty(permissions);

            authorizationEditingService.RemoveEntitiesGroup("Important Accounts");
            UnitOfWork.Current.TransactionalFlush();

            permissions = permissionService.GetPermissionsFor(user);
            Assert.IsEmpty(permissions);
        }

        [Test]
        public void CanRemoveOperation()
        {
            authorizationEditingService.RemoveOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Assert.IsNull(authorizationEditingService.GetOperationByName("/Account/Edit"));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), "Cannot remove operation '/Account' because it has child operations. Remove those operations and try again.")]
        public void CannotRemoveParentOperatio()
        {
            authorizationEditingService.RemoveOperation("/Account");
        }

        [Test]
        public void CanRemoveNestedOperation()
        {
            authorizationEditingService.RemoveOperation("/Account/Edit");
            UnitOfWork.Current.TransactionalFlush();
            Operation parent = authorizationEditingService.GetOperationByName("/Account");

            Assert.AreEqual(0, parent.Children.Count);
        }
    }
}