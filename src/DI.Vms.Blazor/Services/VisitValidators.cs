using DI.Vms.Blazor.Api;
using FluentValidation;
using FluentValidation.Results;

namespace DI.Vms.Blazor.Services;

/// <summary>
/// The rule sets a save is validated in, named rather than spelled at the call sites.
///
/// There are three rather than one because <b>order is part of the behaviour</b>. A save
/// carries a card read, a photographed card or typed details, and which fields can be
/// judged depends on which of those arrived - so the checks have always run in three
/// places: the shape of the request, then whatever the chosen provenance allows, then the
/// two fields every path must supply.
///
/// Collapsing them into one pass would change what a client is told. A request with no
/// mobile number and an unverifiable card read would start reporting the mobile number,
/// and the tablet - which decides whether to send the officer back to the card by the
/// refusal's title - would act on the wrong one. So the sets are declared together here
/// and run where their checks already ran.
/// </summary>
public static class VisitRules
{
    /// <summary>The request on its own: the visit fields, and one provenance.</summary>
    public const string Request = nameof(Request);

    /// <summary>What reception typed, when that is where the identity came from.</summary>
    public const string TypedIdentity = nameof(TypedIdentity);

    /// <summary>Asked of every path, after the card has been read.</summary>
    public const string Contact = nameof(Contact);
}

/// <summary>
/// Everything about a <see cref="SaveVisitRequest"/> that can refuse it.
///
/// DI-IT-POL-AIDEV-001 §4.4 requires input validation on every server-side entry point to
/// be expressed with FluentValidation. This is that, and it is deliberately a <b>wrapper
/// and not a reimplementation</b>: the judgements still live in
/// <see cref="VisitorFields"/>, which is what the desk screens and the tablet's own copy
/// are written against. A second opinion about what a valid Emirates ID number is, living
/// in a validator, is exactly the drift that put three different answers in three places
/// before <see cref="VisitorFields"/> existed.
///
/// What changes is that the rules are now <b>declared</b> rather than written as a run of
/// early returns, so they can be read in one place, tested on their own, and listed in a
/// review without reading the endpoint.
/// </summary>
public sealed class SaveVisitRequestValidator : AbstractValidator<SaveVisitRequest>
{
    public SaveVisitRequestValidator()
    {
        /* One failure, not a list. The endpoint answers with a single sentence a person at
           a desk can act on, and it always has; collecting every fault and returning the
           first would only mean doing work nobody reads. */
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleSet(VisitRules.Request, () =>
        {
            RuleFor(r => r.EntityId)
                .GreaterThan(0)
                .WithMessage("An entity is required.");

            RuleFor(r => r.PersonToVisit)
                .NotEmpty()
                .WithMessage("A person to visit is required.");

            RuleFor(r => r.Purpose)
                .NotEmpty()
                .WithMessage("A purpose is required.");

            /* The free-text box that only exists for one choice. Stored apart from Purpose
               so the report can still group by it - see VisitorEntry.PurposeOther. */
            RuleFor(r => r.PurposeOther)
                .NotEmpty()
                .When(r => r.Purpose == VisitPurposes.Other)
                .WithMessage("Details are required when the purpose is Other.");

            /*  One entry, one provenance.
             *
             *  Two of them in one body is not a request to merge; it is a client that has
             *  lost track of which path it is on, and the saved record would claim a
             *  provenance it cannot support. The two messages differ because the two
             *  mistakes differ, and both are kept exactly as they were. */
            RuleFor(r => r)
                .Must(r => r.ReadResponseXml is null && r.Manual is null)
                .When(r => r.MrzText is { Length: > 0 })
                .WithMessage("Send a card read, a photographed card, or typed details - one of them.");

            RuleFor(r => r)
                .Must(r => r.ReadResponseXml is null)
                .When(r => r.MrzText is not { Length: > 0 } && r.Manual is not null)
                .WithMessage("Send either a card read or manual details, not both.");
        });

        RuleSet(VisitRules.TypedIdentity, () =>
        {
            /*  The number, judged rather than counted.
             *
             *  Custom rather than Must, so VisitorFields decides both whether this is
             *  acceptable and what to say about it - which of "that is 14 digits", "that
             *  does not begin 784" and "that fails its check digit" a person needs is not
             *  something to work out twice. It is also the one typed field that blocks: a
             *  wrong ID number does not make a bad record, it makes a second person. */
            RuleFor(r => r.Manual)
                .Custom((manual, context) =>
                {
                    if (VisitorFields.TypedIdNumberProblem(manual?.IdNumber) is { } problem)
                    {
                        context.AddFailure(problem);
                    }
                });

            /* Null-safe on Manual although this set only runs where it is not null. A rule
               that throws when it is asked the wrong question is a rule that turns a
               refusal into a 500, and the set is one `IncludeRuleSets` away from being
               run somewhere else by someone who has not read this file. */
            RuleFor(r => r.Manual == null ? null : r.Manual.FullNameEnglish)
                .NotEmpty()
                .OverridePropertyName(nameof(ManualIdentity.FullNameEnglish))
                .WithMessage("A name is required.");
        });

        RuleSet(VisitRules.Contact, () =>
        {
            /*  Mandatory, and only here.
             *
             *  The desk browser has required it for a while and the server never did, so a
             *  tablet could record a visitor with no way to reach them. A tablet built
             *  before the field existed is refused by this, with a message naming the
             *  field - the intended trade against finding the gap months later in a
             *  report.
             *
             *  Whether the number looks right is a different question and is not asked
             *  here: VisitorFields.MobileConcern says so on the way past and never
             *  refuses. Reception has somebody standing in front of them. */
            RuleFor(r => r.ContactMobile)
                .NotEmpty()
                .WithMessage("A mobile number is required.");
        });
    }
}

/// <summary>
/// What every card must carry by the time it is written, however it was read.
///
/// Separate from the request because it is not a field a client sends: a chip read, a
/// photographed zone and a typed form all arrive differently and all end up as
/// <see cref="CardData"/>, so this is the one place the rule can be asked once and apply
/// to all three. Name and ID number are not here - what makes those acceptable differs by
/// path, which is the whole reason <see cref="VisitRules.TypedIdentity"/> is its own set.
/// </summary>
public sealed class CardIdentityValidator : AbstractValidator<CardData>
{
    public CardIdentityValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.ExpiryDate)
            .NotEmpty()
            .WithMessage("The card expiry date is required.");
    }
}

public static class ValidatorExtensions
{
    /// <summary>
    /// The first thing wrong, or null - which is the shape the endpoint answers in.
    ///
    /// FluentValidation returns a list and the API returns one sentence, so the bridge is
    /// here rather than repeated at four call sites. With
    /// <see cref="CascadeMode.Stop"/> set on both validators the list holds at most one
    /// entry anyway; taking the first is belt and braces, and keeps the declaration order
    /// in this file the order a client is told about things.
    /// </summary>
    /// <param name="ruleSets">
    /// Which sets to run. None means the validator's default set, which is what
    /// <see cref="CardIdentityValidator"/> uses - it has no sets to choose between.
    /// </param>
    public static string? FirstProblem<T>(
        this IValidator<T> validator, T instance, params string[] ruleSets)
    {
        var context = ruleSets.Length == 0
            ? new ValidationContext<T>(instance)
            : ValidationContext<T>.CreateWithOptions(
                instance, options => options.IncludeRuleSets(ruleSets));

        ValidationResult result = validator.Validate(context);

        return result.IsValid ? null : result.Errors[0].ErrorMessage;
    }
}
