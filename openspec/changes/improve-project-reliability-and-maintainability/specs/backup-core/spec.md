# Spec Delta

## Purpose

Define reliable, symmetric backup and restore behavior so that save data can be copied, listed, loaded, and deleted without loss of structure or silent corruption.

## ADDED Requirements

### Requirement: Create backup preserves save content and structure
The system SHALL create a backup by copying the selected save source into a named backup location while preserving the directory structure and file contents present in the source.

#### Scenario: Successful backup creation
- **WHEN** the user requests a new backup with a valid name and the save source exists and is readable
- **THEN** the system creates the backup containing the same relative file paths and contents as the source
- **AND** the backup becomes available in the backup list

#### Scenario: Backup name is invalid or already exists
- **WHEN** the user requests a backup with an empty name, an invalid path name, or a name that already exists
- **THEN** the system rejects the operation without modifying existing backups
- **AND** the system reports an actionable reason

### Requirement: Restore backup preserves backup content and structure
The system SHALL restore a selected backup to the selected target save location while preserving the backup's directory structure and file contents.

#### Scenario: Successful restore
- **WHEN** the user requests to load an existing backup and the target location is writable
- **THEN** the system copies all backed-up files to the target preserving relative paths
- **AND** any files overwritten are replaced with the backed-up versions

#### Scenario: Restore source or target is unavailable
- **WHEN** the selected backup does not exist or the target location cannot be written
- **THEN** the system does not report success
- **AND** the system reports the failure and any recovery guidance available

### Requirement: List and delete backups reflect actual storage
The system SHALL list backups from the resolved backup location for the selected game/user context and SHALL delete only the explicitly selected backup.

#### Scenario: List available backups
- **WHEN** the backup location for the current context exists
- **THEN** the system displays the backups present in that location

#### Scenario: Confirmed backup deletion
- **WHEN** the user confirms deletion of a selected backup
- **THEN** the system removes that backup and updates the list
- **AND** other backups remain unchanged

### Requirement: Destructive and failure-prone operations are explicit
The system MUST obtain explicit user confirmation before deleting a backup or performing an overwrite-restore when existing target data may be replaced, and MUST surface operation failures clearly.

#### Scenario: User cancels destructive operation
- **WHEN** the user cancels a delete or overwrite confirmation prompt
- **THEN** the system performs no changes

#### Scenario: Operation fails during execution
- **WHEN** a backup, restore, or delete operation fails
- **THEN** the system reports that the operation failed with relevant context
- **AND** the system does not indicate successful completion
