using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ChatServer.Models.Entities;

namespace ChatServer.Data
{
    public partial class ChatAPPContext : DbContext
    {
        public ChatAPPContext()
        {
        }

        public ChatAPPContext(DbContextOptions<ChatAPPContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Channel> Channels { get; set; } = null!;
        public virtual DbSet<ChannelMembership> ChannelMemberships { get; set; } = null!;
        public virtual DbSet<FileUpload> FileUploads { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:DatabaseAWS");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Channel>(entity =>
            {
                entity.ToTable("Channel");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<ChannelMembership>(entity =>
            {
                entity.ToTable("ChannelMembership");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ChannelId).HasColumnName("channel_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Channel)
                    .WithMany(p => p.ChannelMemberships)
                    .HasForeignKey(d => d.ChannelId)
                    .HasConstraintName("FK__ChannelMe__chann__412EB0B6");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChannelMemberships)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__ChannelMe__user___403A8C7D");
            });

            modelBuilder.Entity<FileUpload>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Filename)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("filename");

                entity.Property(e => e.Path)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("path");

                entity.Property(e => e.Size).HasColumnName("size");

                entity.Property(e => e.UploadedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("uploaded_at");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ChannelId).HasColumnName("channel_id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.ReceiverStatus)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("receiver_status");

                entity.Property(e => e.SendTime)
                    .HasColumnType("datetime")
                    .HasColumnName("send_time");

                entity.Property(e => e.SenderStatus)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("sender_status");

                entity.Property(e => e.Type)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("type");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Channel)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ChannelId)
                    .HasConstraintName("FK__Message__channel__47DBAE45");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Message__user_id__3B75D760");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IsRead).HasColumnName("is_read");

                entity.Property(e => e.MessageId).HasColumnName("message_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Message)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.MessageId)
                    .HasConstraintName("FK__Notificat__messa__440B1D61");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Notificat__user___44FF419A");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FileId).HasColumnName("file_id");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("username");

                entity.HasOne(d => d.File)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.FileId)
                    .HasConstraintName("FK__Users__file_id__38996AB5");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
