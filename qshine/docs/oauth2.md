# OAuth2 Service

## References

1. [*OAuth2 Official site*](https://oauth.net/2/)
2. [*Sample SharpOAuth2*](https://github.com/ghorsey/SharpOAuth2)
3. [*Nuget OAuth2 package*](https://www.nuget.org/packages/OAuth2)



## User authentication and authorization

The application should not coupling with a specific user authentication and authorization component. The application need run in a safe environment which protected by security system.

A comprehensive security system is hard to build by application developer. The best security approch of application development is allowing 3rd-party security system integrated to your application through pluggable componnet.
Implement a specific security integration provider is much easy and safe and is also extendable and replaceable.

OAuth2 is a very common security protocal to delegate user authentication and resource authorization tasks to one or many 3rd-party system. You could find a list of OAuth2 providers from [here](https://en.wikipedia.org/wiki/List_of_OAuth_providers).

To integrate OAuth2 protocal into your application you need consider following topic:

### User Authentication

The application is no long manage user credentials in local application system. But the application still need manage application specific user data. Those app specific user data could be common personal data initially retrieve from OAuth2 provider such as birth, email, phone number, person identity picture.
Try to keep data in OAuth2 provider to avoid data sync. In most cases, the application need keep a copy of user data from OAuth2 provider and extend and update data in local.
Remember, application never store user credential in application local.

#### Login

User login is a first step to access protected application resource. Below is application login process workflow.

1. If user hasn't login before, ask for login. 
2. If user login at least once, but user session has been expired, re-login again or refresh the expired session by considering security level and user experience.
3. If user already login and session has not expired, update the session and continue application access.

#### Logout

User logout process is used to clear up user login footprint and session data. The application need send logout request to OAuth2 provider logout endpoint.

#### Resume/Refresh User Session

User application session begins when a user logs into the application and ends when user logs out or session expired. A session could be resumed through a refresh token.
After the session resumed, a new session id should be granted. 
A session may not be used by application if the application do not want to track data within the session. no session resume required for the application.



