using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ChatAPP.API.Contexts
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
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:ChatAPP");
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
                    .HasConstraintName("FK__ChannelMe__chann__2C3393D0");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChannelMemberships)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__ChannelMe__user___2B3F6F97");
            });

            modelBuilder.Entity<FileUpload>(entity =>
            {
                entity.ToTable("FileUpload");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Path)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("path");

                entity.Property(e => e.Size).HasColumnName("size");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.Type)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("type");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Message__user_id__267ABA7A");
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
                    .HasConstraintName("FK__Notificat__messa__2F10007B");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Notificat__user___300424B4");
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
                    .HasConstraintName("FK__Users__file_id__5AEE82B9");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
