using Blog.Shared.Functional;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Infrastructure;

public partial class SysDbContext(DbContextOptions<SysDbContext> options) :  DbContext(options);