using Microsoft.EntityFrameworkCore;

namespace Velusia.Client.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }
}
