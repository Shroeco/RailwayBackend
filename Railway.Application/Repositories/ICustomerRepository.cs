using Railway.Domain.Entities;

namespace Railway.Application.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid customerId);

    Task AddAsync(Customer customer);
}