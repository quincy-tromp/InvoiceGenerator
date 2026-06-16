using Bogus;
using InvoiceGenerator.Api.Contracts;

namespace InvoiceGenerator.Api.Services;

public sealed class InvoiceFactory
{
    public async Task<Invoice> CreateAsync(CancellationToken cancellationToken = default)
    {
        var faker = new Faker();

        // Simulate invoice generation time
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);

        return new Invoice
        {
            Number = faker.Random.Number(100_000, 1_000_000).ToString(),
            IssuedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            SellerAddress = new Address
            {
                CompanyName = faker.Company.CompanyName(),
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
                Email = faker.Internet.Email()
            },
            CustomerAddress = new Address
            {
                CompanyName = faker.Company.CompanyName(),
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                PostalCode = faker.Address.ZipCode(),
                Email = faker.Internet.Email()
            },
            LineItems = Enumerable
                .Range(1, faker.Random.Number(3, 12))
                .Select(i => new LineItem
                {
                    Id = i,
                    Name = faker.Commerce.ProductName(),
                    Quantity = faker.Random.Decimal(10, 1000),
                    Price = faker.Random.Decimal(1, 100)
                })
                .ToArray()
        };
    }
}
