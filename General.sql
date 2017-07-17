USE [General]
GO
/****** Object:  Table [dbo].[t_Site_SocialAccount]    Script Date: 2017/7/16 18:43:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Site_SocialAccount](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[FacebookPageId] [nvarchar](max) NULL,
	[TwitterUserId] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.t_Site_SocialAccount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
