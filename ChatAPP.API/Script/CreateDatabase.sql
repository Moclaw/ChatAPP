Create Database ChatAPP
Go 

Use ChatAPP
Go

-- Tạo bảng User
CREATE TABLE Users (
  id int IDENTITY(1,1) PRIMARY KEY,
  username varchar(255) NOT NULL,
  password varchar(255) NOT NULL
);

-- Tạo bảng Message
CREATE TABLE Message (
  id int IDENTITY(1,1) PRIMARY KEY,
  content text NOT NULL,
  type varchar(255) NOT NULL,
  user_id int FOREIGN KEY REFERENCES Users(id)
);

-- Tạo bảng Channel
CREATE TABLE Channel (
  id int IDENTITY(1,1) PRIMARY KEY,
  name varchar(255) NOT NULL
);

-- Tạo bảng ChannelMembership
CREATE TABLE ChannelMembership (
  id int IDENTITY(1,1) PRIMARY KEY,
  user_id int FOREIGN KEY REFERENCES Users(id),
  channel_id int FOREIGN KEY REFERENCES Channel(id)
);

-- Tạo bảng Notification
CREATE TABLE Notification (
  id int IDENTITY(1,1) PRIMARY KEY,
  message_id int FOREIGN KEY REFERENCES Message(id),
  user_id int FOREIGN KEY REFERENCES Users(id),
  is_read bit NOT NULL
);
CREATE TABLE FileUpload (
  id int IDENTITY(1,1) PRIMARY KEY,
  name varchar(255) NOT NULL,
  path varchar(255) NOT NULL,
  size int NOT NULL,
  user_id int FOREIGN KEY REFERENCES Users(id)
);
