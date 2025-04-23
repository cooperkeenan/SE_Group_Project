# Branching Strategy
Branches should be created for any new feature development under `feature/feature-name`.\
Once a feature is complete, the `development` branch should be merged into the feature branch and the feature tested.\
Once tested, the feature itself should be merged into the development branch.\
Once the development branch is stable, it should be merged into the `main` branch.

## Bugs
Once a bug is identified, an issue should be created and tagged as a `Bug`.\
Bugs should be fixed by creating a branch from the `development` branch under `bugs/issue-number` (e.g. `bugs/24`).\
Once a bug has been fixed, the `development` branch should be merged into the bug branch and the bug tested.\
Once the bug has been tested, it can be merged into the `development` branch.\
Once the development branch is stable, it should be merged into the `main` branch.

## Naming Conventions
- `main` - The main branch of the repository. This branch should always be stable.
- `development` - The development branch of the repository. This may not be stable but must be tested before merging into the `main` branch.
- `feature/feature-name` - A branch for a new feature. This branch should be created from the `development` branch and merged into the `development` branch once the feature is complete.
- `bugs/issue-number` - A branch for a bug fix. This branch should be created from the `development` branch and merged into the `development` branch once the bug is fixed.

## Pull Requests
Pull requests should be created for all branches before merging into the `development` or `main` branches.\
Pull requests should be reviewed by at least one other developer before merging.

## Code Reviews
Code reviews should be conducted for all pull requests.\
Code reviews should be conducted by at least one other developer.\
