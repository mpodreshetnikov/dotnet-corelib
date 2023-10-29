# CoreLib

## Utilities

### Argument utilities
Use `ArgumentUtils` to conveniently check arguments in methods.

### Atomic utilities
Check `AtomicUtils` for different convenient simple functions.

## Features

### Encryption
If you need to save values in the database that you don't want to be readable,
you can use the `Encryption` feature to automatically encrypt and decrypt values.
#### Installation
1) Provide implementation `ICryptoConverter` service in DI container
(you can use `DefaultCryptoConverter` implementation).
2) Save keys for encryption in a place that is not accessible to the public. Don't lose it!
3) Add `Encryption` feature to your `DbContext` to any `string` property:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.Entity<User>()
        .Property(u => u.SecretString)
        .EncryptedWith(cryptoConverter, maxLength: 20);
}
```
4) Enjoy!
#### What to do if there is data in the database that was not encrypted?
You can use `EncryptionMigration` to encrypt all data in the database.
1) Add type of `Migration` to the `EnryptedWith` method,
where the property was firstly encrypted.
Create new migration to provide here if needed (migration may be empty).
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.Entity<User>()
		.Property(u => u.SecretString)
		.EncryptedWith(cryptoConverter, maxLength: 20, migrationType: typeof(Migrations.MigrationFromSecretStringMustBeEncrypted));
}
```
2) Run updated `Migrate` method of `DbContext`
to encrypt all existing data in the database during migration.							
It may take a long time if there is a lot of data to encrypt in the database.
```csharp
await dbContext.MigrateWithEncryptingMigratorAsync();
```