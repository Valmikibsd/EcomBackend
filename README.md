# Ecom Backend — .NET 9 Microservices (Dapper + Onion Architecture)

## Solution Structure

```
ecom-backend/
├── EcomSolution.sln
├── start-backend.ps1              ← Run this to launch all services
├── scripts/
│   └── 01_Init_Ecom_Database.sql  ← Idempotent DB setup (tables + procs + seed)
└── src/
    ├── Shared/
    │   └── SharedKernel/          ← IDbConnectionFactory, SqlConnectionFactory, ApiResponse<T>
    ├── Gateways/
    │   └── ApiGateway/            ← YARP reverse proxy  :5173 (frontend connects here)
    └── Services/
        ├── Auth/
        │   ├── Auth.Domain/       ← User entity
        │   ├── Auth.Application/  ← IAuthService, IUserRepository, ITokenService, DTOs
        │   ├── Auth.Infrastructure← Dapper repos, JwtTokenService, DI
        │   └── Auth.Api/          ← AuthController  :5001
        ├── Product/
        │   ├── Product.Domain/    ← ProductEntity
        │   ├── Product.Application← IProductService, IProductRepository, DTOs
        │   ├── Product.Infrastructure ← Dapper repo, DI
        │   └── Product.Api/       ← ProductsController  :5002
        └── Order/
            ├── Order.Domain/      ← OrderEntity, OrderItemEntity
            ├── Order.Application/ ← IOrderService, IOrderRepository, DTOs
            ├── Order.Infrastructure ← Dapper repo (transactional), DI
            └── Order.Api/         ← OrdersController  :5003
```

## Architecture — Onion (per service)

```
Presentation (API) → Application Core → Domain Core
                           ↓
                   Infrastructure (Dapper + SQL Server)
```

Each service owns its DI registration via `DependencyInjection.cs` in the Infrastructure layer.

## Database

- **Server**: `172.16.0.30,1433`  
- **Database**: `saksham`  
- **Credentials**: `dbdeveloper / maha@123`

Tables created by the init script:
- `Users` (extended with Email, RefreshToken, RefreshTokenExpiryTime)  
- `Products` (16 records seeded from the Angular dummy-products data)  
- `Orders`, `OrderItems`

Stored procedures created:
- `USP_User_GetByUsernameOrEmail`, `USP_User_Register`, `USP_User_UpdateRefreshToken`, `USP_User_GetByRefreshToken`  
- `USP_Product_GetAll`, `USP_Product_GetById`, `USP_Product_Insert`, `USP_Product_Update`, `USP_Product_Delete`  
- `USP_Order_GetByUserId`, `USP_Order_GetById`  
- Existing `USP_User_Login`, `USP_Employee_*` preserved

## Running the Backend

```powershell
# From ecom-backend folder:
.\start-backend.ps1
```

This opens 4 PowerShell windows:

| Service | URL | Swagger |
|---|---|---|
| Auth | http://localhost:5001 | http://localhost:5001/swagger |
| Product | http://localhost:5002 | http://localhost:5002/swagger |
| Order | http://localhost:5003 | http://localhost:5003/swagger |
| **ApiGateway** | **http://localhost:5173** | — |

## API Endpoints (via Gateway)

### Auth — `/api/auth`
| Method | Endpoint | Body | Description |
|---|---|---|---|
| POST | `/api/auth/login` | `{ username, password }` | Login → returns `accessToken`, `refreshToken` |
| POST | `/api/auth/register` | `{ username, email, password }` | Register new user |
| POST | `/api/auth/refresh` | `{ refreshToken }` | Rotate tokens |

### Products — `/api/products`
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/products` | List all (supports `?search=&category=&brand=&minPrice=&maxPrice=&inStockOnly=`) |
| GET | `/api/products/{id}` | Get by ID |
| POST | `/api/products` | Create (admin) |
| PUT | `/api/products/{id}` | Update (admin) |
| DELETE | `/api/products/{id}` | Delete (admin) |

### Orders — `/api/orders`
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/orders` | Create order with items |
| GET | `/api/orders/{id}` | Get order by ID |
| GET | `/api/orders/user/{userId}` | Get orders for a user |

## Frontend Connection

The Angular app (`ecom-frontend`) is configured in:
- `src/app/config/api.ts` — `API_URL = 'http://localhost:5173/'`
- `src/app/app.config.ts` — `MockApiInterceptor` is disabled

**Default login**: `admin` / `admin123` (from seeded `Users` table)

## Building

```powershell
dotnet build EcomSolution.sln -m:1
```
