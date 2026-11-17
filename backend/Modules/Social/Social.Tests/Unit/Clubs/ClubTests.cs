using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura.", ["/api/media/images/planinari.jpg"]);

    private static List<string> CreateImages(int count) =>
        Enumerable.Range(1, count).Select(number => $"/api/media/images/slika-{number}.jpg").ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_club_requires_a_name(string name)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, name, "Klub ljubitelja planinarskih tura.", CreateImages(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_club_requires_a_description(string description)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", description, CreateImages(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void New_club_requires_one_to_five_images(int imageCount)
    {
        var creation = () => new Club(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura.", CreateImages(imageCount));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Novi opis kluba.", 1)]
    [InlineData("Novo ime", "   ", 1)]
    [InlineData("Novo ime", "Novi opis kluba.", 0)]
    [InlineData("Novo ime", "Novi opis kluba.", 6)]
    public void Updated_club_must_remain_valid(string name, string description, int imageCount)
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.Explorer, name, description, CreateImages(imageCount));

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_update_the_club()
    {
        var club = CreateClub();

        var update = () => club.Update(WellKnownUsers.SecondExplorer, "Novo ime", "Novi opis kluba.", CreateImages(1));

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_delete_the_club()
    {
        var club = CreateClub();

        var deletion = () => club.EnsureCanBeDeletedBy(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void New_club_has_its_owner_among_members()
    {
        var club = CreateClub();

        club.Members.Should().ContainSingle(member => member.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public void Member_of_the_club_cannot_request_to_join_it()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);
        club.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        var joining = () => club.RequestToJoin(WellKnownUsers.SecondExplorer);

        joining.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_with_a_pending_request_cannot_send_another_request_to_the_same_club()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var joining = () => club.RequestToJoin(WellKnownUsers.SecondExplorer);

        joining.Should().Throw<DomainException>();
    }

    [Fact]
    public void Withdrawing_a_request_that_does_not_exist_is_rejected()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.ThirdExplorer);

        var withdrawal = () => club.WithdrawRequest(WellKnownUsers.SecondExplorer);

        withdrawal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Accepted_request_makes_the_requester_a_member_and_disappears()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        club.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.SecondExplorer);
        club.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Rejected_request_disappears_and_the_requester_is_not_a_member()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        club.RejectRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Accepting_a_request_that_does_not_exist_is_rejected()
    {
        var club = CreateClub();

        var acceptance = () => club.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        acceptance.Should().Throw<NotFoundException>();
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Rejecting_a_request_that_does_not_exist_is_rejected()
    {
        var club = CreateClub();

        var rejection = () => club.RejectRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        rejection.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_accept_a_request()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.ThirdExplorer);

        var acceptance = () => club.AcceptRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer);

        acceptance.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_reject_a_request()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.ThirdExplorer);

        var rejection = () => club.RejectRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer);

        rejection.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Member_of_the_club_cannot_be_invited()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        club.AcceptInvitation(WellKnownUsers.SecondExplorer);

        var invitation = () => club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        invitation.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_with_a_pending_request_cannot_be_invited()
    {
        var club = CreateClub();
        club.RequestToJoin(WellKnownUsers.SecondExplorer);

        var invitation = () => club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        invitation.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_with_a_pending_invitation_cannot_be_invited_again()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        var invitation = () => club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        invitation.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_with_a_pending_invitation_cannot_request_to_join_the_same_club()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        var joining = () => club.RequestToJoin(WellKnownUsers.SecondExplorer);

        joining.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_invite()
    {
        var club = CreateClub();

        var invitation = () => club.Invite(WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer);

        invitation.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_withdraw_an_invitation()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.ThirdExplorer);

        var withdrawal = () => club.WithdrawInvitation(WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer);

        withdrawal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_remove_a_member()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.ThirdExplorer);
        club.AcceptInvitation(WellKnownUsers.ThirdExplorer);

        var removal = () => club.RemoveMember(WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer);

        removal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Accepted_invitation_makes_the_invitee_a_member_and_disappears()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.AcceptInvitation(WellKnownUsers.SecondExplorer);

        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.SecondExplorer);
        club.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Declined_invitation_disappears_and_the_invitee_is_not_a_member()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        club.DeclineInvitation(WellKnownUsers.SecondExplorer);

        club.Invitations.Should().NotContain(invitation => invitation.UserId == WellKnownUsers.SecondExplorer);
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Withdrawing_an_invitation_that_does_not_exist_is_rejected()
    {
        var club = CreateClub();

        var withdrawal = () => club.WithdrawInvitation(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

        withdrawal.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Accepting_an_invitation_that_does_not_exist_is_rejected()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.ThirdExplorer);

        var acceptance = () => club.AcceptInvitation(WellKnownUsers.SecondExplorer);

        acceptance.Should().Throw<NotFoundException>();
        club.Members.Should().NotContain(member => member.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void Declining_an_invitation_that_does_not_exist_is_rejected()
    {
        var club = CreateClub();
        club.Invite(WellKnownUsers.Explorer, WellKnownUsers.ThirdExplorer);

        var declining = () => club.DeclineInvitation(WellKnownUsers.SecondExplorer);

        declining.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void Owner_cannot_remove_themselves_from_the_club()
    {
        var club = CreateClub();

        var removal = () => club.RemoveMember(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        removal.Should().Throw<DomainException>();
        club.Members.Should().Contain(member => member.UserId == WellKnownUsers.Explorer);
    }
}
