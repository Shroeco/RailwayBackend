using Microsoft.EntityFrameworkCore;
using Railway.Application.Repositories;
using Railway.Domain.Entities;
using Railway.Infrastructure.Data;

namespace Railway.Infrastructure.Persistence;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly RailwayDbContext _context;

    public CustomerRepository(RailwayDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid customerId)
    {
        return await _context.Customers.SingleOrDefaultAsync(customer => customer.Id == customerId);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }
}