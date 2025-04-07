# JWT 토큰
JWT 토큰은 유저의 "신원"이나 "권한"을 결정하는 정보를 담고 있는 데이터 조각이다. <br/>
클라이언트와 서버는 이 JWT 토큰을 사용하여 통신한다. <br/>
JWT 토큰은 비밀키(개인키, 대칭키)를 사용하여 암호화하여 사용한다. <br/><br/>

다만, 인증 받지 않은 자가 이 JWT 토큰을 탈취하여 본인인양 사용하는 경우 문제가 될 수 있다. <br/>
서버에서는 탈취자와 본 주인을 구분할 수 없기에 유효 기간을 두는 방식으로 리스크를 줄인다. <br/>
유효 기간을 짧게 두면 사용자가 로그인을 자주 해야하기에 사용자 경험이 좋지 않고 <br/>
유효 기간을 길게 두면 보안 위험이 크다. <br/>
이를 위해 유효기간이 다른 JWT 토큰을 2개 운용하는 방식이 있다. (Access Token + Refresh Token) <br/>

# Access Token + Refresh Token 방식
### Access Token
권한을 보여주기 위해 주로 사용하는 토큰이며 리프레시 토큰으로 로그인 없이 새로 발급받는다. <br/>
짧은 유효 기간을 가지고 있다. 주로 30분 내외의 만료 기간 <br/>
### Refresh Token
액세스 토큰을 발행하기 위한 토큰 <br/>
긴 유효 기간을 가지고 있다. 2주에서 한 달 정도의 만료 기간 <br/>

### 작동 예제로 이해
클라이언트가 로그인 인증에 성공하여 Refresh Token, Access Token 2개를 서버로부터 발급받는다. <br/>
클라이언트는 발급 받은 Refresh Token과 Access Token을 로컬에 저장한다. <br/>
이후 클라이언트는 헤더에 Access Token을 포함하여 통신을 한다. (Authoriztion) <br/>
일정 기간이 지나 Access Token의 유효 기간이 만료된다. <br/>
Access Token은 더 이상 유효하지 않으므로 권한 없는 사용자가 되며, 서버는 401 (Unauthorized) 를 응답한다. <br/>
401을 받은 클라이언트는 invalid_token(유효기간만료)를 확인하고 헤더에 Access Token 대신 Refresh Token을 넣어 재요청한다. <br/>
Refresh Token으로 사용자의 권한을 확인한 서버는 응답 쿼리 헤더에 새로운 Access Token을 넣어 응답한다. <br/>
위 과정에서 만약 Refresh Token도 만료된 상황이라면 서버는 동일하게 401 응답하고 클라이언트는 재로그인하여야 한다. <br/>

# Access Token 방식

# 추가 내용


출처 : 
https://velog.io/@chuu1019/Access-Token%EA%B3%BC-Refresh-Token%EC%9D%B4%EB%9E%80-%EB%AC%B4%EC%97%87%EC%9D%B4%EA%B3%A0-%EC%99%9C-%ED%95%84%EC%9A%94%ED%95%A0%EA%B9%8C - JWT 및 동작 방식 이해 <br/>
https://blog.ull.im/engineering/2019/02/07/jwt-strategy.html - 추가 내용 <br/>

<hr/>



