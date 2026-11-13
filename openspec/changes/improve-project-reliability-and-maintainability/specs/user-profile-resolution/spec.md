# Spec Delta

## Purpose

Define how save profile identifiers are discovered and labeled so users can distinguish profiles reliably, with optional external name resolution that does not block core backup management.

## ADDED Requirements

### Requirement: Discover user/profile contexts from save storage
The system SHALL identify available user or profile contexts for a selected game from the resolved save storage when the game's save model is user/profile-based.

#### Scenario: Multiple profile directories exist
- **WHEN** the selected game's save location contains multiple profile directories
- **THEN** the system lists each profile as a selectable context

#### Scenario: Save location has no discoverable profiles
- **WHEN** the resolved save location is missing or contains no discoverable profile contexts
- **THEN** the system presents an empty or unavailable profile state without crashing
- **AND** the user can still access actions that do not require a discovered profile where supported

### Requirement: Profile labels use cache and optional external resolution
The system SHALL display a stable label for each profile using locally available information and MAY enrich it with externally resolved names; cached labels MUST be reusable without requiring repeated external lookups.

#### Scenario: Cached label is available
- **WHEN** a cached label exists for a profile identifier
- **THEN** the system displays the cached label without performing an external lookup

#### Scenario: External name resolution succeeds
- **WHEN** no cached label exists and external name resolution returns a name
- **THEN** the system displays the resolved label and persists it to the local cache/configuration contract

### Requirement: External resolution failures do not block core workflows
The system MUST allow core profile selection and backup workflows to continue when external name resolution is unavailable, slow, or fails.

#### Scenario: External service unavailable
- **WHEN** external name resolution fails due to network or service errors
- **THEN** the system falls back to a non-external label for the profile
- **AND** backup listing and operations remain available for the selected context
