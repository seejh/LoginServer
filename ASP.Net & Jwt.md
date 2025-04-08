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
Authorization 탭에서 Type을 Bearer Token으로 변경, 발급받은 액세스 토큰 삽입<br/>
이후 Send 버튼을 누르면 200 응답과 함께 Response Body에 모든 사용자의 리프레시 토큰 목록을 보여준다. <br/>

### API 테스트 : 특정 사용자의 모든 리프레시 토큰 조회
특정 사용자(여기선 1번 사용자)의 모든 리프레시 토큰(활성, 만료, 취소) 조회 <br/>
HTTP GET 요청, URL : http://localhost:4000/users/1/refresh-tokens <br/>
Authorization 탭에서 

출처 : <br/>
https://jasonwatmore.com/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api#running-angular <br/>
