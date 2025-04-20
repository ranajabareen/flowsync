﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplicationFlowSync.Models
{
    public class AppUser : IdentityUser
    { 
    [Required, MaxLength(50)]
    public string FirstName { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; }

    public UserStatus Status { get; set; } = UserStatus.OnDuty;


    public string? Major { get; set; }

    [Required, MaxLength(20)]
    public Role Role { get; set; } // "Leader" أو "Member"

    public string? PictureURL { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }
    public string? LeaderID { get; set; }


    [ForeignKey("LeaderID")]
    public AppUser? Leader { get; set; }

    public ICollection<AppUser>? TeamMembers { get; set; }

    // العلاقة مع المهام
    public ICollection<Task>? Tasks { get; set; }

    // العلاقة مع التقارير
    public ICollection<Report>? Reports { get; set; }

    //الطلبات التي أرسلها هذا المستخدم كـ Member
    public ICollection<PendingMemberRequest>? SentJoinRequests { get; set; }

    // الطلبات التي استلمها هذا المستخدم كـ Leader
    public ICollection<PendingMemberRequest>? ReceivedJoinRequests { get; set; }
}

public enum Role
{
    Leader,
    Member
}

public enum UserStatus
{
    Temporarilyleave,
    Annuallyleave,
    OnDuty
  }
}
