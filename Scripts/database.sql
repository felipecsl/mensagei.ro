USE [tagsd]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloudTalk_Role](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CloudTalk_Role] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[CloudTalk_Role] ON
INSERT [dbo].[CloudTalk_Role] ([ID], [Name]) VALUES (1, N'Member')
INSERT [dbo].[CloudTalk_Role] ([ID], [Name]) VALUES (2, N'Moderator')
INSERT [dbo].[CloudTalk_Role] ([ID], [Name]) VALUES (3, N'Assistant')
SET IDENTITY_INSERT [dbo].[CloudTalk_Role] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloudTalk_ChatRoomType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CloudTalk_ChatRoomType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[CloudTalk_ChatRoomType] ON
INSERT [dbo].[CloudTalk_ChatRoomType] ([ID], [Type]) VALUES (1, N'OneToOne')
INSERT [dbo].[CloudTalk_ChatRoomType] ([ID], [Type]) VALUES (2, N'ManyToMany')
SET IDENTITY_INSERT [dbo].[CloudTalk_ChatRoomType] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CloudTalk_Client](
	[Email] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Picture] [image] NULL,
	[Username] [varchar](36) NULL,
	[Password] [varchar](36) NULL,
	[RoleID] [int] NOT NULL,
 CONSTRAINT [PK_CloudTalk_Client] PRIMARY KEY CLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloudTalk_Event](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SenderID] [nvarchar](128) NOT NULL,
	[RecipientID] [nvarchar](128) NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
	[EventType] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CloudTalk_Message] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloudTalk_Tracking](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Data] [nvarchar](2048) NOT NULL,
	[ClientEmail] [nvarchar](128) NULL,
 CONSTRAINT [PK_CloudTalk_Events] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CloudTalk_ChatRoom](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[TypeID] [int] NOT NULL,
	[RequiresAuthentication] [bit] NOT NULL,
 CONSTRAINT [PK_CloudTalk_ChatRoom] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
SET IDENTITY_INSERT [dbo].[CloudTalk_ChatRoom] ON
INSERT [dbo].[CloudTalk_ChatRoom] ([ID], [Name], [TypeID], [RequiresAuthentication]) VALUES (153, N'TestRoom', 1, 0)
INSERT [dbo].[CloudTalk_ChatRoom] ([ID], [Name], [TypeID], [RequiresAuthentication]) VALUES (154, N'Web App Test Room', 1, 0)
SET IDENTITY_INSERT [dbo].[CloudTalk_ChatRoom] OFF
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CloudTalk_Client_ChatRoom](
	[ClientID] [nvarchar](128) NOT NULL,
	[ChatRoomID] [int] NOT NULL,
 CONSTRAINT [PK_CloudTalk_Client_ChatRoom] PRIMARY KEY CLUSTERED 
(
	[ClientID] ASC,
	[ChatRoomID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CloudTalk_Event] ADD  CONSTRAINT [DF_CloudTalk_Event_EventType]  DEFAULT (N'Message') FOR [EventType]
GO
ALTER TABLE [dbo].[CloudTalk_ChatRoom]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_ChatRoom_CloudTalk_ChatRoomType] FOREIGN KEY([TypeID])
REFERENCES [dbo].[CloudTalk_ChatRoomType] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CloudTalk_ChatRoom] CHECK CONSTRAINT [FK_CloudTalk_ChatRoom_CloudTalk_ChatRoomType]
GO
ALTER TABLE [dbo].[CloudTalk_Client]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_Client_CloudTalk_Role] FOREIGN KEY([RoleID])
REFERENCES [dbo].[CloudTalk_Role] ([ID])
GO
ALTER TABLE [dbo].[CloudTalk_Client] CHECK CONSTRAINT [FK_CloudTalk_Client_CloudTalk_Role]
GO
ALTER TABLE [dbo].[CloudTalk_Client_ChatRoom]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_Client_ChatRoom_CloudTalk_ChatRoom] FOREIGN KEY([ChatRoomID])
REFERENCES [dbo].[CloudTalk_ChatRoom] ([ID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CloudTalk_Client_ChatRoom] CHECK CONSTRAINT [FK_CloudTalk_Client_ChatRoom_CloudTalk_ChatRoom]
GO
ALTER TABLE [dbo].[CloudTalk_Client_ChatRoom]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_Client_ChatRoom_CloudTalk_Client] FOREIGN KEY([ClientID])
REFERENCES [dbo].[CloudTalk_Client] ([Email])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CloudTalk_Client_ChatRoom] CHECK CONSTRAINT [FK_CloudTalk_Client_ChatRoom_CloudTalk_Client]
GO
ALTER TABLE [dbo].[CloudTalk_Event]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_Message_CloudTalk_Client] FOREIGN KEY([SenderID])
REFERENCES [dbo].[CloudTalk_Client] ([Email])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CloudTalk_Event] CHECK CONSTRAINT [FK_CloudTalk_Message_CloudTalk_Client]
GO
ALTER TABLE [dbo].[CloudTalk_Event]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_Message_CloudTalk_Client1] FOREIGN KEY([RecipientID])
REFERENCES [dbo].[CloudTalk_Client] ([Email])
GO
ALTER TABLE [dbo].[CloudTalk_Event] CHECK CONSTRAINT [FK_CloudTalk_Message_CloudTalk_Client1]
GO
ALTER TABLE [dbo].[CloudTalk_Tracking]  WITH CHECK ADD  CONSTRAINT [FK_CloudTalk_Event_CloudTalk_Client] FOREIGN KEY([ClientEmail])
REFERENCES [dbo].[CloudTalk_Client] ([Email])
GO
ALTER TABLE [dbo].[CloudTalk_Tracking] CHECK CONSTRAINT [FK_CloudTalk_Event_CloudTalk_Client]
GO
