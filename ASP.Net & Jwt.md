# 예제 설명
.NET 6.0 (ASP.NET Core) API에서 Refresh 토큰을 사용하여 JWT 인증을 구현 <br/><br/>
액세스 토큰과 리프레시 토큰 사용, 인증에 성공하면 유효 시간 15분의 액세스 토큰과 http 전용 쿠키에서 7일 후에 만료되는 
리프레시 토큰을 반환한다. 액세스 토큰은 API의 보안 경로에 액세스 하는데 사용되며 리프레시 토큰은 액세스 토큰이 만료될 때
새 액세스 토큰을 생성하는데 사용된다. <br/><br/>

Http 전용 쿠키는 XSS(교차 사이트 스크립팅) 공격을 방지하는 클라이언트 측 자바스크립트에 액세스할 수 없기 때문에 <br/>
보안을 강화하기 위해 리프레시 토큰에 사용되며, 리프레시 토큰은 CSRF(교차 사이트 요청 위조) 공격에 사용되는 것을 방지하는 <br/>
새 액세스 토큰을 생성하는데만 액세스할 수 있다. <br/>

# ASP.NET Core API 엔드포인트
### /users/authenticate
BODY에 사용자 이름과 비밀번호가 포함된 HTTP POST 요청을 받는 공개 경로. <br/>
로그인 기능이며, 성공할 시 기본 사용자 정보와 액세스 토큰, 리프레시 토큰이 포함된 Http 전용 쿠키를 반환한다. <br/>

### /users/refresh-token
리프레시 토큰이 있는 쿠키가 포한된 POST 요청을 수락하는 공개 경로. <br/>
성공할 시, 기본 사용자 정보와 액세스 토큰, 리프레시 토큰이 포함된 Http 전용 쿠키가 반환된다. <br/>

### /users/revoke-token
BODY 또는 쿠키에 리프레시 토큰이 포함된 POST 요청을 수락하는 보안 경로. (둘 다 주어질 경우 REQUEST BODY를 우선한다.) <br/>
성공할 시, 토큰이 취소되며, 더이상 새 액세스 토큰을 생성할 수 없다. <br/>

### /users
이 애플리케이션의 모든 사용자 목록을 반환. <br/>
GET 요청을 수락하는 보안 경로 <br/>

### /users/{id}
지정된 ID를 가진 사용자의 정보를 반환. <br/>
GET 요청을 수락하는 보안 경로 <br/>

### /users/{id}/refresh-tokens
지정된 ID를 가진 사용자의 모든 리프레시 토큰(활성된, 취소된) 목록을 반환. <br/>
GET 요청을 수락하는 보안 경로 <br/>

# 리프레시 토큰 순환 (Refresh Token Rotation)
새 액세스 토큰을 생성하기 위해 리프레시 토큰을 사용할 때마다 리프레시 토큰도 새로 발급한다. 이 기술은 <br/>
리프레시 토큰 로테이션이라고 알려져 있으며 리프레시 토큰의 수명을 줄여서 손상된 토큰이 오랫동안 유효할 가능성을 줄여준다. <br/>
새로 생성된 리프레시 토큰은 해지된 토큰의 필드에 저장된다. <br/>

# 취소된 토큰 재사용 감지
취소된 리프레시 토큰을 사용하여 새 액세스 토큰을 생성하려고 하면 API는 이를 <br/>
도난 당한 리프레시 토큰(활성이든 비활성이든)을 가진 악의적인 사용자로 처리한다. <br/>
이 경우 API는 모든 하위 토큰을 취소한다. 취소 사유는 사용자가 데이터베이스와 요청으로 볼 수 있도록 기록한다. <br/>

# 테스트를 위한 EF Core InMemory DB
API 코드를 가능한 한 간단하게 유지하기 위해 실제 DB 서버를 설치하는 대신 Entity Framework Core가 메모리 내 DB를 만들고 연결할 수 있도록 하는
EF Core InMemory DB Provider 를 사용하도록 구성한다. <br/>
프로그램 시작 시 DB에 테스트 사용자를 만드는 내용은 Program.cs 파일에 있다. <br/>

# Postman을 사용한 .NET API 테스트
### API 테스트 : 사용자 인증
HTTP POST 요청, URL : http://localhost:4000/users/authenticate <br/>
Body 탭에서 raw, JSON 선택 <br/>
{ <br/>
  "username" : "test", <br/>
  "password" : "test" <br/>
} <br/>
이후 Send 버튼을 누르면 유저 정보와 액세스 토큰이 포함된 response body, 리프레시 토큰이 포함된 쿠키와 함께 200 OK 응답을 받는다. <br/>
##### 액세스 토큰
![image](https://github.com/user-attachments/assets/b2949410-0aef-4d7e-b5a8-5bff962023d1)

##### 리프레시 토큰
![image](https://github.com/user-attachments/assets/91653148-b51f-4517-aad9-27e8037bacd3)

### API 테스트 : 토큰 새로 고침
이 단계는 유효한 리프레시 토큰이 필요하기 때문에 위의 사용자 인증을 거친 후에만 수행할 수 있다. <br/>
HTTP POST 요청, URL : http://localhost:4000/users/refresh-token <br/>
이후 Send 버튼을 클릭하면 사용자 정보와 액세스 토큰이 포함된 200 OK 응답을 받는다. <br/>
보안 경로에 액세스하는데 사용할 것이므로 액세스 토큰 값의 복사본을 만든다. <br/>

### API 테스트 : 모든 사용자를 검색 (인증된 요청)
JWT 토큰을 가진 모든 사용자를 조회 <br/>
HTTP GET 요청, URL : http://localhost:4000/users <br/>
Authorization 탭에서 Type을 Bearer Token으로 변경, 발급 받은 액세스 토큰 삽입 <br/>
이후 Send 버튼을 누르면 200 OK 응답과 함께 Response Body에 모든 사용자의 리프레시 토큰 목록을 보여준다. <br/>

### API 테스트 : 특정 사용자의 모든 리프레시 토큰 조회
특정 사용자(여기선 1번 사용자)의 모든 리프레시 토큰(활성, 만료, 취소) 조회 <br/>
HTTP GET 요청, URL : http://localhost:4000/users/1/refresh-tokens <br/>
Authorization 탭에서 Type을 Bearer Token으로 변경, 발급 받은 액세스 토큰 삽입 <br/>
이후 Send 버튼을 누르면 200 OK 응답과 함께 사용자의 모든 리프레시 토큰 목록을 보여준다 <br/>

### API 테스트 : 토큰 취소
새 액세스 토큰을 더 이상 생성할 수 없도록 리프레시 토큰을 취소 <br/>
HTTP POST 요청, URL : localhost:4000/users/revoke-token <br/>
Authorization 탭에서 Type을 Bearer Token으로 변경, 발급 받은 액세스 토큰 삽입 <br/>
Body 탭에서 유형을 raw, JSON 선택 <br/>
{ <br/>
  "token" : "현재 활성화되어 있는 리프레시 토큰" <br/>
} <br/>
이후 Send 버튼을 누르면 200 OK 응답과 함께 Response Body에 Token revoked라는 메세지를 받는다. <br/>
NOTE: You can also revoke the token in the refreshToken cookie with the /users/revoke-token route, <br/> 
to revoke the refresh token in the cookie simply send the same request with an empty body. <br/>

<hr/>

# 프로젝트 구조
### Authorization (인증)
API에서 사용자 정의 JWT 인증 및 권한 부여를 구현하는 클래스를 포함한다. <br/>
AllowAnonymouseAttribute.cs <br/>
AuthorizeAttribute.cs <br/>
JwtMiddleware.cs <br/>
JwtUtils.cs <br/>

### Controller (컨트롤러)
웹 API에 대한 엔드포인트/경로를 정의, 컨트롤러는 클라이언트 응용 프로그램의 http 요청을 받는 웹 API의 게이트웨이이다. <br/>
UsersController.cs <br/>

### Model (모델)
컨트롤러 메서드에 대한 요청 및 응답 모델을 나타내며, 요청 모델은 들어오는 요청에 대한 매개 변수를 정의, <br/>
응답 모델은 반환되는 데이터를 정의한다. <br/>
AuthenticateRequest.cs <br/>
AuthenticateResponse.cs <br/>
RevokeTokenRequest.cs <br/>

### Service (서비스)
비즈니스 로직, 검증, DB 액세스 코드가 포함된다. <br/>
UserService.cs <br/>

### Entity (엔티티)
DB에 저장된 응용 프로그램의 데이터를 나타낸다. <br/>
EF Core(Entity Framework Core)는 DB의 관계형 데이터를 데이터 관리 및 CRUD 작업에 애플리케이션 내에서 <br/>
사용할 C# 엔터티 개체의 인스턴스로 매핑한다. <br/>
RefreshToken.cs <br/>
User.cs <br/>

### Helper
위의 경우에 맞지 않는 모든 것 <br/>
AppException.cs <br/>
AppSettings.cs <br/>
DataContext.cs <br/>
ErrorHandlerMiddleware.cs <br/>

<hr/>

# 각 파일 설명
## Authorization
### 커스텀 익명 허용 특성
AllowAnonymousAttribute.cs <br/>
커스텀 [AllowAnonymouse] 특성은 [Authorize] 특성으로 장식되어 있는 컨트롤러의 특정 메서드에 대한 익명 액세스를 허용하는데 사용 <br/>
UsersController는 [] authenticate 및 리프레시 토큰 작업 메서드에 대한 익명 액세스를 허용하는데 사용한다. <br/>
내장된 AllowAnonymous 특성을 사용하지 않고 사용자 지정으로 만들어 사용하는데 내장된 것과 네임스페이스
충돌을 피해서 사용해야 한다. <br/>

```c#
namespace WebApi.Authorization;

[AttributeUsage(AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute
{ }
```

### 커스텀 인증 특성
AuthorizeAttribute.cs <br/>
커스텀 [Authorize] 특성은 컨트롤러나 컨트롤러의 특정 작업 메서드에 대한 접근을 제한하는데 사용 <br/>
인증된 요청만이 허용될 수 있다. <br/>
컨트롤러가 [Authorize] 특성으로 장식되면 [AllowAnonymous] 특성으로 장식된 작업 메서드들을 제외하고 모든 메서드가 제한된다. <br/>
인증은 현재 요청(context.HttpContext.Items["User"])에 인증된 사용자가 첨부되어 있는지 체크하는 OnAuthorization 메서드에 의해 수행된다. <br/>
요청에 정당한 JWT 액세스 토큰이 있다면 커스텀 JWT 미들웨어에 의해 인증된 사용자로 첨부된다.
인증에 실패하면 401 Unauthorized 응답을 받는다. <br/>

```c#
namespace WebApi.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Entities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 메서드가 [AllowAnonymous] 특성으로 장식되어 있으면 인증 패스
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        // 그렇지 않다면, 인증
        var user = (User)context.HttpContext.Items["User"];
        if (user == null)
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
    }
}
```

### 커스텀 JWT 미들웨어
JwtMiddleware.cs <br/>
커스텀 JWT 미들웨어는 요청 인증 헤더에서 JWT 토큰을 추출하고 jwtUtils.ValidateToken() 메서드로 유효성을 검사한다. <br/> 

```c#
namespace WebApi.Authorization;

using Microsoft.Extensions.Options;
using WebApi.Helpers;
using WebApi.Services;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppSettings _appSettings;

    public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
    {
        _next = next;
        _appSettings = appSettings.Value;
    }

    public async Task Invoke(HttpContext context, IUserService userService, IJwtUtils jwtUtils)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var userId = jwtUtils.ValidateJwtToken(token);
        if (userId != null)
        {
            // attach user to context on successful jwt validation
            context.Items["User"] = userService.GetById(userId.Value);
        }

        await _next(context);
    }
}
```

### JWT 유틸리티
JwtUtils.cs <br/>


<hr/>
출처 : <br/>
https://jasonwatmore.com/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api#running-angular <br/>
