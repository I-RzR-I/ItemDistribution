# USING

To initiate the generation of the new user suggestion call the method `DistributionSuggestionHelper.Instance.GenerateNewDistributionSuggestion(...)`.

For currently working as input information, you must provide:
- `AvoidDuplicateResult` -> This means that as a result, the alternative user suggestion may contain a duplicate equal to the base user suggestion or not. The default value is `false`;
- `MaxAllowedInProcessDocuments` -> This means how many documents/items can be processed by the user in one full working day. the default value is `20`;
- `FullWorkDayHours` -> This means in hours, how many hours are in one full working day. The default value is `8` hours.
- `EligibleRepartitionUsers` - List of users from which we will find new suggestion.

```csharp
/// <summary>
///     Distribution suggestion parameters
/// </summary>
/// <typeparam name="TUserId">Type of user id</typeparam>
public class DistributionParams<TUserId> where TUserId : struct
{
    /// <summary>
    ///     Avoid or do not duplicate in result.
    ///     In case the value is 'true', then 'SuggestionUser' and 'AlternativeUser' must be different data.
    /// </summary>
    public bool AvoidDuplicateResult { get; set; } = false;

    /// <summary>
    ///     Gets or sets the maximum number of documents allowed at the same time in processing.
    /// </summary>
    /// <value></value>
    /// <remarks>Default value is set for maximum 20 documents/items per day.</remarks>
    public int MaxAllowedInProcessDocuments { get; set; } = 20;

    /// <summary>
    ///     Gets or sets number of hours in on a working day.
    /// </summary>
    /// <value></value>
    /// <remarks>Default value is set for 8 hours per day.</remarks>
    public decimal FullWorkDayHours { get; set; } = 8;

    /// <summary>
    ///     Gets or sets eligible users for a new selection.
    /// </summary>
    /// <value></value>
    /// <remarks></remarks>
    public IEnumerable<UserInfoOptions<TUserId>> EligibleRepartitionUsers { get; set; } = new List<UserInfoOptions<TUserId>>();
}
```


For every eligilbe user to distribution you must provide the following information:
- `UserId` -> User unique identifier;
- `UserName` -> User name;
- `WorkingHours` -> How many hours does the user work in one day?;
- `InProcessDocuments` -> The number of items/documents that are in processing for this user;
- `LastActivityDate` -> The user`s last activity date/timestamp.
- `UserPriority` -> User priority. As previously said, it can be: the user role, user qualification, user work experience, etc.
```csharp
/// <summary>
///     User info
/// </summary>
/// <typeparam name="TUserId">Type of user id</typeparam>
public class UserInfoOptions<TUserId>
{
    /// <summary>
    ///     User id
    /// </summary>
    public TUserId UserId { get; set; }

    /// <summary>
    ///     User name
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     User working hours
    /// </summary>
    public decimal WorkingHours { get; set; }

    /// <summary>
    ///     Number of documents/items in processing at this moment
    /// </summary>
    public int InProcessDocuments { get; set; }

    /// <summary>
    ///     Last user activity date
    /// </summary>
    public DateTime LastActivityDate { get; set; }

    /// <summary>
    ///     User select priority.
    ///     The priority is higher as close to 0 as possible is the value.
    /// </summary>
    public short UserPriority { get; set; }
}
```

The result of the new suggestion contains 2 suggestions: the base suggestion and the alternative (a random user suggestion).
```csharp
/// <summary>
///     New item distribution suggestion
/// </summary>
/// <typeparam name="TUserId">Type of user id</typeparam>
public class DistributionSuggestionResultDto<TUserId>
{
    /// <summary>
    ///     Suggested user
    /// </summary>
    public DistributionSuggestionDto<TUserId> SuggestionUser { get; set; }

    /// <summary>
    ///     Alternative user suggested (random user)
    /// </summary>
    public DistributionSuggestionDto<TUserId> AlternativeUser { get; set; }
}
```
