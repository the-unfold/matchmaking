namespace Auth.Data

open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore

type AuthDbContext(options: DbContextOptions<AuthDbContext>)=
    inherit IdentityDbContext(options)
    