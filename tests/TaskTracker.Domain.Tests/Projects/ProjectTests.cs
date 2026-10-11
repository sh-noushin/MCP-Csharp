using TaskTracker.Domain.Common;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Domain.Tests.Projects;

public sealed class ProjectTests
{
    [Fact]
    public void Create_TrimsNameAndStartsActive()
    {
        var project = Project.Create("  Website  ", TestClock.Now);

        Assert.Equal("Website", project.Name.Value);
        Assert.Equal(ProjectStatus.Active, project.Status);
        Assert.Equal(TestClock.Now, project.CreatedAt);
        Assert.Empty(project.Tasks);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingName(string? name)
    {
        var exception = Assert.Throws<DomainException>(() => Project.Create(name, TestClock.Now));

        Assert.Equal("Project name is required.", exception.Message);
    }

    [Fact]
    public void Create_RejectsNameLongerThanMaximum()
    {
        var tooLong = new string('a', ProjectName.MaxLength + 1);

        Assert.Throws<DomainException>(() => Project.Create(tooLong, TestClock.Now));
    }

    [Fact]
    public void Create_AcceptsNameAtMaximumLength()
    {
        var longest = new string('a', ProjectName.MaxLength);

        var project = Project.Create(longest, TestClock.Now);

        Assert.Equal(longest, project.Name.Value);
    }

    [Fact]
    public void Create_GivesEveryProjectItsOwnId()
    {
        var first = Project.Create("One", TestClock.Now);
        var second = Project.Create("Two", TestClock.Now);

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(first.Id.Value.ToString(), first.Id.ToString());
    }

    [Fact]
    public void Archive_MarksProjectArchived()
    {
        var project = Project.Create("Website", TestClock.Now);

        project.Archive();

        Assert.Equal(ProjectStatus.Archived, project.Status);
    }

    [Fact]
    public void Archive_RejectsAlreadyArchivedProject()
    {
        var project = Project.Create("Website", TestClock.Now);
        project.Archive();

        Assert.Throws<DomainException>(project.Archive);
    }

    [Fact]
    public void ProjectName_ComparesByValueAndPrintsItsValue()
    {
        var name = ProjectName.Create("Website");

        Assert.Equal(ProjectName.Create(" Website "), name);
        Assert.Equal("Website", name.ToString());
    }
}
