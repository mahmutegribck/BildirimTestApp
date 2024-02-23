USE [master]
GO
/****** Object:  Database [TestDb]    Script Date: 2024-02-20 14:40:05 ******/
CREATE DATABASE [TestDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TestDb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TestDb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'TestDb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TestDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [TestDb] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TestDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [TestDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TestDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TestDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TestDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TestDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [TestDb] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TestDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TestDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TestDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TestDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TestDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TestDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TestDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TestDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TestDb] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TestDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TestDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TestDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TestDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TestDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TestDb] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TestDb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TestDb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [TestDb] SET  MULTI_USER 
GO
ALTER DATABASE [TestDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TestDb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TestDb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TestDb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [TestDb] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [TestDb] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [TestDb] SET QUERY_STORE = OFF
GO
USE [TestDb]
GO
/****** Object:  Table [dbo].[SisBildirim]    Script Date: 2024-02-20 14:40:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SisBildirim](
	[BildirimId] [int] IDENTITY(1,1) NOT NULL,
	[GonderilecekKullaniciId] [int] NOT NULL,
	[GonderimDurumu] [int] NOT NULL,
	[BildirimIcerikId] [int] NOT NULL,
 CONSTRAINT [PK_SisBildirim] PRIMARY KEY CLUSTERED 
(
	[BildirimId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SisBildirimIcerik]    Script Date: 2024-02-20 14:40:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SisBildirimIcerik](
	[BildirimIcerikId] [int] IDENTITY(1,1) NOT NULL,
	[Json] [varchar](max) NOT NULL,
	[OlusturulanTarih] [datetime] NOT NULL,
	[Aciklama] [varchar](1000) NULL,
 CONSTRAINT [PK_SisBildirimIcerik] PRIMARY KEY CLUSTERED 
(
	[BildirimIcerikId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SisBildirimOutbox]    Script Date: 2024-02-20 14:40:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SisBildirimOutbox](
	[BildirimOutboxId] [int] IDENTITY(1,1) NOT NULL,
	[GonderimDenemeSayisi] [int] NOT NULL,
	[BildirimId] [int] NOT NULL,
 CONSTRAINT [PK_SisBildirimOutbox] PRIMARY KEY CLUSTERED 
(
	[BildirimOutboxId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SisKullanici]    Script Date: 2024-02-20 14:40:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SisKullanici](
	[KullaniciId] [int] IDENTITY(1,1) NOT NULL,
	[KullaniciAdi] [varchar](50) NOT NULL,
	[Rol] [varchar](500) NULL,
 CONSTRAINT [PK_SisKullanici] PRIMARY KEY CLUSTERED 
(
	[KullaniciId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_SisKullanici] UNIQUE NONCLUSTERED 
(
	[KullaniciAdi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SisBildirim]  WITH CHECK ADD  CONSTRAINT [FK_SisBildirim_SisBildirimIcerik] FOREIGN KEY([BildirimIcerikId])
REFERENCES [dbo].[SisBildirimIcerik] ([BildirimIcerikId])
GO
ALTER TABLE [dbo].[SisBildirim] CHECK CONSTRAINT [FK_SisBildirim_SisBildirimIcerik]
GO
ALTER TABLE [dbo].[SisBildirim]  WITH CHECK ADD  CONSTRAINT [FK_SisBildirim_SisKullanici] FOREIGN KEY([GonderilecekKullaniciId])
REFERENCES [dbo].[SisKullanici] ([KullaniciId])
GO
ALTER TABLE [dbo].[SisBildirim] CHECK CONSTRAINT [FK_SisBildirim_SisKullanici]
GO
ALTER TABLE [dbo].[SisBildirimOutbox]  WITH CHECK ADD  CONSTRAINT [FK_SisBildirimOutbox_SisBildirim] FOREIGN KEY([BildirimId])
REFERENCES [dbo].[SisBildirim] ([BildirimId])
GO
ALTER TABLE [dbo].[SisBildirimOutbox] CHECK CONSTRAINT [FK_SisBildirimOutbox_SisBildirim]
GO
USE [master]
GO
ALTER DATABASE [TestDb] SET  READ_WRITE 
GO