namespace contester;

public static class Constants
{
    public const int DefaultMaxUploadFileSizeBytes = 10485760;

    public enum Permission
    {
        ManageContests,
        ManageProblems,
        ManageAttempts,
        ManageContestParticipants,
        ManageSchemaDescriptions,
    }
}