namespace Sard.Infrastructure.Implementation.Group
{
    public class GroupService(
    AppDbContext db,
    UserManager<AppUser> userManager,
    IHubContext<GroupHub> hubContext,
    IImageService imageService) : IGroupService
    {
        public async Task<Result<IEnumerable<GroupDto>>> GetMyGroupsAsync(string userId)
        {
            var groups = await db.GroupMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.Group).ThenInclude(g => g.Members).ThenInclude(gm => gm.User)
                .Include(m => m.Group).ThenInclude(g => g.Creator)
                .Select(m => m.Group)
                .ToListAsync();

            var result = groups.Select(g => MapToDto(g, userId));
            return Result<IEnumerable<GroupDto>>.Success(result);
        }

        public async Task<Result<GroupDto>> GetGroupAsync(int groupId, string userId)
        {
            var group = await db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupDto>.Failure("الجروب غير موجود");

            var isMember = group.Members.Any(m => m.UserId == userId);
            if (!isMember)
                return Result<GroupDto>.Failure("لست عضواً في هذا الجروب");

            return Result<GroupDto>.Success(MapToDto(group, userId));
        }

        public async Task<Result<GroupDto>> CreateGroupAsync(string userId, CreateGroupDto dto)
        {
            var group = new Sard.Domain.Entities.Group
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatorId = userId,
                CreatedAt = EgyptDateTime.Now
            };

            db.Groups.Add(group);
            await db.SaveChangesAsync();

            db.GroupMembers.Add(new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                Role = GroupRole.Admin,
                JoinedAt = EgyptDateTime.Now
            });

            await db.SaveChangesAsync();
            await db.Entry(group).Reference(g => g.Creator).LoadAsync();
            await db.Entry(group).Collection(g => g.Members).LoadAsync();
            foreach (var m in group.Members)
                await db.Entry(m).Reference(x => x.User).LoadAsync();

            return Result<GroupDto>.Success(MapToDto(group, userId));
        }

        public async Task<Result<GroupDto>> AddMemberAsync(string requesterId, int groupId, string targetUserId)
        {
            var group = await db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupDto>.Failure("الجروب غير موجود");

            var requesterMember = group.Members.FirstOrDefault(m => m.UserId == requesterId);
            if (requesterMember is null || requesterMember.Role != GroupRole.Admin)
                return Result<GroupDto>.Failure("ليس لديك صلاحية إضافة أعضاء");

            var isMutual = await db.Follows.AnyAsync(f => f.FollowerId == requesterId && f.FollowedId == targetUserId)
                && await db.Follows.AnyAsync(f => f.FollowerId == targetUserId && f.FollowedId == requesterId);

            if (!isMutual)
                return Result<GroupDto>.Failure("يمكنك فقط إضافة من تتابعهم ويتابعونك");

            if (group.Members.Any(m => m.UserId == targetUserId))
                return Result<GroupDto>.Failure("المستخدم عضو بالفعل");

            var targetUser = await userManager.FindByIdAsync(targetUserId);
            if (targetUser is null)
                return Result<GroupDto>.Failure("المستخدم غير موجود");

            var newMember = new GroupMember
            {
                GroupId = groupId,
                UserId = targetUserId,
                Role = GroupRole.Member,
                JoinedAt = EgyptDateTime.Now,
                User = targetUser
            };

            db.GroupMembers.Add(newMember);
            group.Members.Add(newMember);

            await db.SaveChangesAsync();

            var dto = MapToDto(group, requesterId);
            await BroadcastGroupUpdateAsync(groupId, dto);

            return Result<GroupDto>.Success(dto);
        }

        public async Task<Result<GroupDto>> RemoveMemberAsync(string requesterId, int groupId, string targetUserId)
        {
            var group = await db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupDto>.Failure("الجروب غير موجود");

            var requesterMember = group.Members.FirstOrDefault(m => m.UserId == requesterId);
            if (requesterMember is null || requesterMember.Role != GroupRole.Admin)
                return Result<GroupDto>.Failure("ليس لديك صلاحية");

            if (group.CreatorId == targetUserId)
                return Result<GroupDto>.Failure("لا يمكن إزالة منشئ الجروب");

            var targetMember = group.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (targetMember is null)
                return Result<GroupDto>.Failure("المستخدم ليس عضواً");

            db.GroupMembers.Remove(targetMember);
            group.Members.Remove(targetMember);

            await db.SaveChangesAsync();

            var dto = MapToDto(group, requesterId);
            await BroadcastGroupUpdateAsync(groupId, dto);

            return Result<GroupDto>.Success(dto);
        }

        public async Task<Result<GroupDto>> PromoteToAdminAsync(string requesterId, int groupId, string targetUserId)
        {
            var group = await db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupDto>.Failure("الجروب غير موجود");

            if (group.CreatorId != requesterId)
                return Result<GroupDto>.Failure("فقط منشئ الجروب يمكنه تعيين الأدمن");

            var targetMember = group.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (targetMember is null)
                return Result<GroupDto>.Failure("المستخدم ليس عضواً");

            if (targetMember.Role == GroupRole.Admin)
                return Result<GroupDto>.Failure("العضو أدمن بالفعل");

            targetMember.Role = GroupRole.Admin;
            await db.SaveChangesAsync();

            var dto = MapToDto(group, requesterId);
            await BroadcastGroupUpdateAsync(groupId, dto);

            return Result<GroupDto>.Success(dto);
        }

        public async Task<Result<GroupDto>> DemoteFromAdminAsync(string requesterId, int groupId, string targetUserId)
        {
            var group = await db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupDto>.Failure("الجروب غير موجود");

            if (group.CreatorId != requesterId)
                return Result<GroupDto>.Failure("فقط منشئ الجروب يمكنه إلغاء صلاحية الأدمن");

            if (group.CreatorId == targetUserId)
                return Result<GroupDto>.Failure("لا يمكن إلغاء صلاحية منشئ الجروب");

            var targetMember = group.Members.FirstOrDefault(m => m.UserId == targetUserId);
            if (targetMember is null)
                return Result<GroupDto>.Failure("المستخدم ليس عضواً");

            if (targetMember.Role != GroupRole.Admin)
                return Result<GroupDto>.Failure("العضو ليس أدمن أصلاً");

            targetMember.Role = GroupRole.Member;
            await db.SaveChangesAsync();

            var dto = MapToDto(group, requesterId);
            await BroadcastGroupUpdateAsync(groupId, dto);

            return Result<GroupDto>.Success(dto);
        }

        public async Task<Result<GroupDto>> ToggleLockAsync(string requesterId, int groupId)
        {
            var group = await db.Groups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.Creator)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupDto>.Failure("الجروب غير موجود");

            var member = group.Members.FirstOrDefault(m => m.UserId == requesterId);
            if (member is null || member.Role != GroupRole.Admin)
                return Result<GroupDto>.Failure("ليس لديك صلاحية");

            group.IsLocked = !group.IsLocked;
            await db.SaveChangesAsync();

            var dto = MapToDto(group, requesterId);
            await BroadcastGroupUpdateAsync(groupId, dto);

            return Result<GroupDto>.Success(dto);
        }

        public async Task<Result<IEnumerable<GroupMemberDto>>> GetMutualsAsync(string userId, int groupId)
        {
            var existingMemberIds = await db.GroupMembers
                .Where(m => m.GroupId == groupId)
                .Select(m => m.UserId)
                .ToListAsync();

            var mutuals = await db.Follows
                .Where(f => f.FollowerId == userId &&
                    db.Follows.Any(f2 => f2.FollowerId == f.FollowedId && f2.FollowedId == userId) &&
                    !existingMemberIds.Contains(f.FollowedId))
                .Include(f => f.Followed)
                .Select(f => new GroupMemberDto(
                    0, f.Followed.Id, f.Followed.DisplayName,
                    f.Followed.ProfileImageUrl, "Member", DateTime.MinValue))
                .ToListAsync();

            return Result<IEnumerable<GroupMemberDto>>.Success(mutuals);
        }

        public async Task<Result<GroupMessageDto>> SendMessageAsync(string userId, int groupId, SendMessageDto dto)
        {
            var group = await db.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
                return Result<GroupMessageDto>.Failure("الجروب غير موجود");

            if (group.IsLocked)
                return Result<GroupMessageDto>.Failure("الجروب مقفل حالياً");

            var isMember = group.Members.Any(m => m.UserId == userId);
            if (!isMember)
                return Result<GroupMessageDto>.Failure("لست عضواً في هذا الجروب");

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Result<GroupMessageDto>.Failure("المستخدم غير موجود");

            var message = new GroupMessage
            {
                Content = dto.Content,
                GroupId = groupId,
                SenderId = userId,
                CreatedAt = EgyptDateTime.Now
            };

            db.GroupMessages.Add(message);
            await db.SaveChangesAsync();

            message.Sender = user;
            var messageDto = MapMessageToDto(message, userId);

            await SafeBroadcastAsync(groupId, "NewGroupMessage", messageDto);

            return Result<GroupMessageDto>.Success(messageDto);
        }

        public async Task<Result<IEnumerable<GroupMessageDto>>> GetMessagesAsync(string userId, int groupId, int page)
        {
            var isMember = await db.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (!isMember)
                return Result<IEnumerable<GroupMessageDto>>.Failure("لست عضواً");

            var messages = await db.GroupMessages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.Sender)
                .Include(m => m.Reactions).ThenInclude(r => r.User)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * 50)
                .Take(50)
                .ToListAsync();

            var anyChanged = false;
            foreach (var m in messages)
                anyChanged |= RemoveDuplicateReactions(m);

            if (anyChanged)
                await db.SaveChangesAsync();

            var result = messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => MapMessageToDto(m, userId));

            return Result<IEnumerable<GroupMessageDto>>.Success(result);
        }

        public async Task<Result<GroupMessageDto>> ReactToMessageAsync(string userId, int groupId, int messageId, string emoji)
        {
            var isMember = await db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (!isMember)
                return Result<GroupMessageDto>.Failure("لست عضواً في هذا الجروب");

            if (string.IsNullOrWhiteSpace(emoji))
                return Result<GroupMessageDto>.Failure("الرمز غير صالح");

            var message = await db.GroupMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.GroupId == groupId);

            if (message is null)
                return Result<GroupMessageDto>.Failure("الرسالة غير موجودة");

            if (message.IsDeleted)
                return Result<GroupMessageDto>.Failure("لا يمكن التفاعل مع رسالة محذوفة");

            
            if (RemoveDuplicateReactions(message))
                await db.SaveChangesAsync();

            var existing = message.Reactions.FirstOrDefault(r => r.UserId == userId);
            if (existing is not null)
            {
                if (existing.Emoji == emoji)
                {
                    
                    db.GroupMessageReactions.Remove(existing);
                    message.Reactions.Remove(existing);
                }
                else
                {
                    
                    existing.Emoji = emoji;
                }
            }
            else
            {
                var currentUser = message.Sender?.Id == userId ? message.Sender : await userManager.FindByIdAsync(userId);
                var reaction = new GroupMessageReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    User = currentUser,
                    Emoji = emoji,
                    CreatedAt = EgyptDateTime.Now
                };
                db.GroupMessageReactions.Add(reaction);
                message.Reactions.Add(reaction);
            }

            await db.SaveChangesAsync();

            var dto = MapMessageToDto(message, userId);
            await SafeBroadcastAsync(groupId, "MessageReacted", dto);

            return Result<GroupMessageDto>.Success(dto);
        }

        public async Task<Result<GroupMessageDto>> DeleteMessageAsync(string userId, int groupId, int messageId)
        {
            var requesterMember = await db.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (requesterMember is null)
                return Result<GroupMessageDto>.Failure("لست عضواً في هذا الجروب");

            var message = await db.GroupMessages
                .Include(m => m.Sender)
                .Include(m => m.Reactions).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == messageId && m.GroupId == groupId);

            if (message is null)
                return Result<GroupMessageDto>.Failure("الرسالة غير موجودة");

            var isOwner = message.SenderId == userId;
            var isAdmin = requesterMember.Role == GroupRole.Admin;

            if (!isOwner && !isAdmin)
                return Result<GroupMessageDto>.Failure("ليس لديك صلاحية حذف هذه الرسالة");

            db.GroupMessageReactions.RemoveRange(message.Reactions);
            message.Reactions.Clear();
            message.IsDeleted = true;
            message.Content = string.Empty;

            await db.SaveChangesAsync();

            var dto = MapMessageToDto(message, userId);
            await SafeBroadcastAsync(groupId, "MessageDeleted", dto);

            return Result<GroupMessageDto>.Success(dto);
        }

        private bool RemoveDuplicateReactions(GroupMessage message)
        {
            var duplicateGroups = message.Reactions
                .GroupBy(r => r.UserId)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicateGroups.Count == 0) return false;

            foreach (var grp in duplicateGroups)
            {
                var extras = grp.OrderBy(r => r.CreatedAt).Skip(1).ToList();
                foreach (var extra in extras)
                {
                    db.GroupMessageReactions.Remove(extra);
                    message.Reactions.Remove(extra);
                }
            }

            return true;
        }

        private async Task BroadcastGroupUpdateAsync(int groupId, GroupDto dto)
        {
            await SafeBroadcastAsync(groupId, "GroupUpdated", dto);
        }

        private async Task SafeBroadcastAsync(int groupId, string eventName, object payload)
        {
            try
            {
                await hubContext.Clients.Group($"group-{groupId}").SendAsync(eventName, payload);
            }
            catch
            {
            }
        }

        private GroupDto MapToDto(Sard.Domain.Entities.Group g, string currentUserId)
        {
            var members = (g.Members ?? new List<GroupMember>())
                .OrderByDescending(m => m.UserId == g.CreatorId)
                .ThenByDescending(m => m.Role == GroupRole.Admin)
                .ThenBy(m => m.JoinedAt)
                .Select(m => new GroupMemberDto(
                    m.Id, m.UserId, m.User?.DisplayName ?? "", m.User?.ProfileImageUrl,
                    m.Role.ToString(), m.JoinedAt))
                .ToList();

            return new GroupDto(
                g.Id, g.Name, g.Description, g.ImageUrl, g.IsLocked,
                g.CreatorId, g.Creator?.DisplayName ?? "",
                g.Members?.Count ?? 0,
                members.FirstOrDefault(m => m.UserId == currentUserId),
                g.CreatedAt,
                members);
        }

        private GroupMessageDto MapMessageToDto(GroupMessage m, string currentUserId)
        {
            var reactionGroups = m.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new MessageReactionSummaryDto(
                    g.Key,
                    g.Count(),
                    g.Any(r => r.UserId == currentUserId),
                    g.Select(r => r.User?.DisplayName ?? "").Where(n => !string.IsNullOrWhiteSpace(n)).ToList()))
                .OrderByDescending(r => r.Count)
                .ToList();

            var myReaction = m.Reactions.FirstOrDefault(r => r.UserId == currentUserId)?.Emoji;

            return new GroupMessageDto(
                m.Id, m.Content, m.SenderId, m.Sender?.DisplayName ?? "", m.Sender?.ProfileImageUrl,
                m.GroupId, m.CreatedAt, m.IsDeleted, reactionGroups, myReaction);
        }
    }
}