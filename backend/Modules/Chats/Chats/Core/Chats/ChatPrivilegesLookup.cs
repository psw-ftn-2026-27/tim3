using Exploration.Contracts;

namespace Chats.Core.Chats;

// Assembles the Privileges of one chat for the caller (P1), shared by moderation and the message query.
// A tour chat: the author of the tour, asked through the Exploration contract.
// A challenge chat: the caller, when the caller is an administrator. Only the caller's role is known,
// so a target administrator cannot be recognised (an accepted limitation).
// A conversation of two users: nobody.
public sealed class ChatPrivilegesLookup
{
    private readonly IExplorationApi _explorationApi;

    public ChatPrivilegesLookup(IExplorationApi explorationApi)
    {
        _explorationApi = explorationApi;
    }

    public async Task<Privileges> GetAsync(Subject subject, Guid callerId, bool callerIsAdministrator)
    {
        switch (subject.Kind)
        {
            case SubjectKind.Tour:
                var authorId = await _explorationApi.GetTourAuthorIdAsync(subject.FirstId);
                return authorId is null ? new Privileges([]) : new Privileges([authorId.Value]);
            case SubjectKind.Challenge:
                return callerIsAdministrator ? new Privileges([callerId]) : new Privileges([]);
            default:
                return new Privileges([]);
        }
    }
}
