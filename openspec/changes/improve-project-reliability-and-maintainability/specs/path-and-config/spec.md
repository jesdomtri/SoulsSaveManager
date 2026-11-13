# Spec Delta

## Purpose

Define deterministic path resolution and consistent configuration behavior so save locations, backup locations, and persisted settings are reliable across launches and environments.

## ADDED Requirements

### Requirement: Resolve game and user paths deterministically
The system SHALL resolve save and backup paths using explicit, rooted filesystem paths derived from the selected game, user/profile context, and configured locations rather than from ambiguous relative segments or current working directory assumptions.

#### Scenario: Standard game save path resolution
- **WHEN** a supported game with a known default save location is selected
- **THEN** the system resolves the save path to an absolute path appropriate for that game and user context

#### Scenario: Backup root is independent of working directory
- **WHEN** the application is launched from different working directories
- **THEN** the system resolves the same configured backup root and game/user backup paths

### Requirement: Configuration uses a single consistent contract
The system MUST read and write application configuration through a single, consistently named settings file and schema so that cached user labels and custom locations persist reliably.

#### Scenario: Persisted custom location is reused
- **WHEN** a user sets or updates a custom save location that the application supports persisting
- **THEN** the system stores it in the application configuration
- **AND** subsequent launches resolve that location from the same configuration contract

#### Scenario: Configuration read or write fails
- **WHEN** the configuration file cannot be read or written
- **THEN** the system reports the failure
- **AND** the system does not silently discard the intended update

### Requirement: Migration from legacy path/config behavior is safe
The system SHALL provide a defined migration or compatibility behavior for previously created backups and settings affected by legacy relative-path or inconsistent configuration naming, when such data is detected.

#### Scenario: Legacy backups are detected
- **WHEN** existing backups are found in a legacy location supported by migration/compatibility rules
- **THEN** the system makes them discoverable or guides the user through migration without deleting them automatically

#### Scenario: Legacy configuration naming is detected
- **WHEN** a legacy settings file name or location is detected and the current contract expects a different one
- **THEN** the system applies the defined compatibility or migration behavior and preserves existing values
