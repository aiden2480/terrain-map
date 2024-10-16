﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TerrainMap.Models;

public record Profile(
    [property: JsonPropertyName("group")] Group Group,
    [property: JsonPropertyName("member")] Member Member,
    [property: JsonPropertyName("unit")] Unit? Unit);

public record Group(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles);

public record Member(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles);

public record Unit(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles,
    [property: JsonPropertyName("section")] string Section);

public record Approval(
    [property: JsonPropertyName("achievement")] ApprovalAchievement Achievement,
    [property: JsonPropertyName("submission")] Submission Submission,
    [property: JsonPropertyName("member")] ApprovalMember Member);

public record ApprovalAchievement(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("achievement_meta")] AchievementMeta Meta);

public record AchievementMeta(
    [property: JsonPropertyName("stage")] int Stage,
    [property: JsonPropertyName("stream")] string Stream,
    [property: JsonPropertyName("branch")] string Branch,
    [property: JsonPropertyName("sia_area")] string SIAArea);

public record Submission(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("actioned_by")] IEnumerable<SubmissionActionedBy> ActionedBy);

public record SubmissionActionedBy(
    [property: JsonPropertyName("member_id")] string Id,
    [property: JsonPropertyName("member_first_name")] string FirstName,
    [property: JsonPropertyName("member_last_name")] string LastName,
    [property: JsonPropertyName("outcome")] string Outcome,
    [property: JsonPropertyName("time")] DateTime Time,
    [property: JsonPropertyName("comment")] string? Comment);

public record ApprovalMember(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("last_name")] string LastName);

public record Achievement(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("member_id")] string MemberId,
    [property: JsonPropertyName("template")] string Template,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("answers")] IReadOnlyDictionary<string, object> Answers);

public record ApprovalInput(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("dialog_text")] string? DialogText,
    [property: JsonPropertyName("selections")] IEnumerable<ApprovalInputSelection>? Selections,
    [property: JsonPropertyName("tabs")] IEnumerable<ApprovalInputTab>? Tabs);

public record ApprovalInputTab(
    [property: JsonPropertyName("selections")] IEnumerable<ApprovalInputSelection> Selections);

public record ApprovalInputSelection(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title);
