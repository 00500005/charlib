# Patterns and Conventions
This guide introduces major internal patterns and conventions that we use throughout charlib. These patterns and conventions provide clear guidelines for contributors, and help with project usability, discovery and further design.

## Type Keys
Type keys are a major pattern used throughout. The purpose of a type key is to provide type agreement and casting information after types have been erased.

By convention, all type keys use a string id as a unique identifier. This id is the sole primary key. Common sub-conventions include inferring an id by the supplied type, though passing a user defined id should be preferred in most cases.

