# API Documentation

## Auth

### Login

**URL**: `/api/Auth/Login`

**Method**: POST

**Description**: Log in a user using their username and password.

**Request Body**:
- `LoginPostModel` (object):
  - `userName` (string): Username
  - `password` (string): Password

**Response**:
- `token` (string): Authentication token after successful login.

---

### Register

**URL**: `/api/Auth/Register`

**Method**: POST

**Description**: Register a new user account.

**Request Body**:
- `RegisterPostModel` (object):
  - `username` (string): Username
  - `password` (string): Password
  - `confirmPassword` (string): Confirm password
  - `fileId` (int): ID of the user's file

**Response**:
- `user` (object): Information of the newly registered user account.

---

### Get User Profile

**URL**: `/api/Auth/GetProfile`

**Method**: GET

**Description**: Get the profile information of the currently authenticated user.

**Response**:
- `user` (object): Profile information of the currently authenticated user.

---

### Get User by Username

**URL**: `/api/Auth/GetUser`

**Method**: GET

**Description**: Get user information based on their username.

**Query Parameters**:
- `username` (string): Username

**Response**:
- `user` (object): User information with the corresponding username.

---

### Get User by ID

**URL**: `/api/Auth/GetUserById`

**Method**: GET

**Description**: Get user information based on their user ID.

**Query Parameters**:
- `userId` (int): User ID

**Response**:
- `user` (object): User information with the corresponding user ID.

## Channel

### Create New Channel

**URL**: `/api/Channel`

**Method**: POST

**Description**: Create a new channel.

**Request Body**:
- `ChannelPostModel` (object):
  - `name` ([...]): Channel name
  - `userId` ([...]): User ID
  - `assignMember` ([...]): Assigned members

**Response**:
- `channel` (object): Information of the newly created channel.

---

### Delete Channel

**URL**: `/api/Channel`

**Method**: DELETE

**Description**: Delete a channel based on the channel ID.

**Query Parameters**:
- `channelId` (int): ID of the channel to be deleted.

**Response**:
- `message` (string): Confirmation message of channel deletion.

---

### Get List of Channels

**URL**: `/api/Channel`

**Method**: GET

**Description**: Get a list of all channels.

**Response**:
- `channels` (array): List of channels.

---

### Send Message in Channel

**URL**: `/api/Channel/{channelId}/messages`

**Method**: POST

**Description**: Send a message in a specific channel.

**Request Body**:
- `MessagePostModel` (object):
  - `content` ([...]): Message content
  - `type` ([...]): Message type
  - `userId` ([...]): User ID
  - `sendTime` ([...]): Send time

**Response**:
- `message` (object): Information of the sent message.

---

### Get Messages in Channel

**URL**: `/api/Channel/{channelId}/messages`

**Method**: GET

**Description**: Get a list of messages in a specific channel.

**Query Parameters**:
- `channelId` (int): Channel ID

**Response**:
- `messages` (array): List of messages in the channel.

---

### Get User's Channels

**URL**: `/api/Channel/{userId}/channel`

**Method**: GET

**Description**: Get a list of channels that a user is a member of.

**Query Parameters**:
- `userId` (int): User ID

**Response**:
- `channels` (array): List of channels that the user is a member of.

---

### Update Channel Status

**URL**: `/api/Channel/{channelId}/updateStatus`

**Method**: PUT

**Description**: Update the status of a channel (e.g., online status, activity status).

**Request Body**:
- `status` ([...]): New status of the channel

**Query Parameters**:
- `channelId` (int): Channel ID

**Response**:
- `channel` (object): Updated channel information.

---

### Get Channel Information

**URL**: `/api/Channel/{channelId}`

**Method**: GET

**Description**: Get detailed information about a channel based on the channel ID.

**Query Parameters**:
- `channelId` (int): Channel ID

**Response**:
- `channel` (object): Channel information.

## Notification

### Get Notifications

**URL**: `/api/Notification`

**Method**: GET

**Description**: Get a list of notifications for the current user.

**Response**:
- `notifications` (array): List of notifications.

---

### Create New Notification

**URL**: `/api/Notification`

**Method**: POST

**Description**: Create a new notification.

**Request Body**:
- `MessagePostModel` (object):
  - `content` ([...]): Notification content
  - `type` ([...]): Notification type
  - `userId` ([...]): User ID
  - `sendTime` ([...]): Send time

**Response**:
- `notification` (object): Information of the newly created notification.