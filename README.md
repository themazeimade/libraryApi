# Library API

Simple AI that handles everyday operations in a library. The features are:

1. Secure User authentication via JWT.
2. Book loan system that keeps track which user has which loaned book copy.
Also it keeps track of the return date and delay fees.
3. Users can check room availability using search filters. Available time 
slots are shown in 30-minute intervals for each room.

The API correctly implements secure standards for authentication. It also 
encrypts all server communication through https. Any http request will be
redirected to the https server.
