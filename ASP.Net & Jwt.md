# 예제 설명
.NET 6.0 (ASP.NET Core) API에서 Refresh 토큰을 사용하여 JWT 인증을 구현 <br/><br/>
액세스 토큰과 리프레시 토큰 사용, 인증에 성공하면 유효 시간 15분의 액세스 토큰과 http 전용 쿠키에서 7일 후에 만료되는 
리프레시 토큰을 반환한다. 액세스 토큰은 API의 보안 경로에 액세스 하는데 사용되며 리프레시 토큰은 액세스 토큰이 만료될 때
새 액세스 토큰을 생성하는데 사용된다. <br/><br/>

Http 전용 쿠키는 XSS(교차 사이트 스크립팅) 공격을 방지하는 클라이언트 측 자바스크립트에 액세스할 수 없기 때문에
보안을 강화하기 위해 리프레시 토큰에 사용되며, 리프레시 토큰은 CSRF(교차 사이트 요청 위조) 공격에 사용되는 것을 방지하는 
새 액세스 토큰을 생성하는데만 액세스할 수 있다.

# ASP.NET Core API 엔드포인트
### /users/authenticate
BODY에 사용자 이름과 비밀번호가 포함된 HTTP POST 요청을 받는 공개 경로.
로그인 기능이며, 성공할 시 기본 사용자 정보와 액세스 토큰, 리프레시 토큰이 포함된 Http 전용 쿠키를 반환한다.

### /users/refresh-token
리프레시 토큰이 있는 쿠키가 포한된 POST 요청을 수락하는 공개 경로.
성공할 시, 기본 사용자 정보와 액세스 토큰, 리프레시 토큰이 포함된 Http 전용 쿠키가 반환된다.

### /users/revoke-token
BODY 또는 쿠키에 리프레시 토큰이 포함된 POST 요청을 수락하는 보안 경로. (둘 다 주어질 경우 REQUEST BODY를 우선한다.)
성공할 시, 토큰이 취소되며, 더이상 새 액세스 토큰을 생성할 수 없다.

### /users
이 애플리케이션의 모든 사용자 목록을 반환.
GET 요청을 수락하는 보안 경로

### /users/{id}
지정된 ID를 가진 사용자의 정보를 반환.
GET 요청을 수락하는 보안 경로

### /users/{id}/refresh-tokens
지정된 ID를 가진 사용자의 모든 리프레시 토큰(활성된, 취소된) 목록을 반환.
GET 요청을 수락하는 보안 경로

# 리프레시 토큰 순환 (Refresh Token Rotation)
새 액세스 토큰을 생성하기 위해 리프레시 토큰을 사용할 때마다 리프레시 토큰도 새로 발급한다. 이 기술은
리프레시 토큰 로테이션이라고 알려져 있으며 리프레시 토큰의 수명을 줄


출처 : <br/>
https://jasonwatmore.com/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api#running-angular <br/>
