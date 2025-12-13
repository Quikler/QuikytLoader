<objective>
Conduct a comprehensive analysis of where and how to introduce the Result pattern in the QuikytLoader codebase to replace exception-based error handling and null returns. Create a detailed migration plan with custom Result<T> implementation, concrete code examples, and step-by-step implementation strategy.

This analysis will guide architectural improvements to make error handling more explicit, type-safe, and aligned with functional programming principles.
</objective>

<context>
QuikytLoader is a .NET 9 Avalonia UI application for downloading YouTube videos as MP3 and sending them to Telegram. The codebase follows Clean Architecture with clear separation between Application (use cases) and Infrastructure (services) layers.

Current error handling approach:
- Infrastructure services throw exceptions for failure cases
- Some methods return null to indicate failure
- Use cases must catch exceptions and handle error propagation to ViewModels
- Error handling logic is scattered and implicit

Files to examine:
@QuikytLoader.Infrastructure/Telegram/TelegramBotService.cs
@QuikytLoader.Infrastructure/YouTube/YouTubeDownloadService.cs
@QuikytLoader.Infrastructure/YouTube/YoutubeExtractor.cs
@QuikytLoader.Application/UseCases/DownloadAndSendUseCase.cs
@QuikytLoader.Application/UseCases/CheckDuplicateUseCase.cs
@CLAUDE.md

Read the CLAUDE.md file to understand project conventions, architecture patterns, and dependency injection setup.
</context>

<analysis_requirements>
Thoroughly analyze and document:

1. **Current Error Handling Patterns**
   - Catalog all exception types thrown in Infrastructure services
   - Identify all null-return scenarios in service methods
   - Map error propagation flow from Infrastructure → Application → Presentation
   - Document implicit error handling assumptions

2. **Result Pattern Design**
   - Design custom Result<T> and Result<T, TError> types appropriate for this codebase
   - Define error type hierarchy (should errors be strings, enums, custom classes?)
   - Specify factory methods (Success, Failure) and pattern matching helpers
   - Consider integration with existing async/await patterns

3. **Migration Candidates**
   - For each service method, evaluate:
     * Current error handling mechanism
     * Whether Result pattern is appropriate (some exceptions should remain!)
     * Proposed Result signature
     * Impact on calling code
   - Prioritize methods by migration value/complexity ratio

4. **Code Examples**
   - Show before/after for at least 3 representative methods:
     * TelegramBotService.SendAudioAsync
     * YouTubeDownloadService.DownloadAsync
     * YoutubeExtractorService.ExtractVideoIdAsync
   - Demonstrate use case layer changes (DownloadAndSendUseCase)
   - Show ViewModel error handling patterns

5. **Breaking Changes Analysis**
   - Identify interface changes required
   - Document impact on dependency injection configuration
   - Note any ViewModel/UI layer adaptations needed

6. **Migration Strategy**
   - Phase 1, 2, 3 breakdown with clear boundaries
   - Parallel running strategy (can old and new patterns coexist during migration?)
   - Testing approach for each phase
   - Rollback contingencies
</analysis_requirements>

<constraints>
- Result pattern should NOT replace all exceptions - only predictable, domain-level failures
  * WHY: Unexpected system failures (OutOfMemoryException, etc.) should still throw
  * Configuration errors at startup can still throw
  * Focus on operational failures: network errors, invalid input, not found, etc.

- Maintain backward compatibility during migration where possible
  * WHY: This allows incremental rollout without breaking existing functionality

- Consider async method signatures carefully
  * Result pattern must work cleanly with Task<Result<T>>
  * Avoid Task<Result<Task<T>>> or other nested monads

- Keep the solution aligned with Clean Architecture principles
  * Infrastructure should not depend on Application layer error types
  * Result types should be defined in Domain or a shared kernel
</constraints>

<output>
Create a comprehensive analysis document:
`./analyses/result-pattern-migration-plan.md`

Structure:
1. Executive Summary (benefits, risks, recommendation)
2. Current State Analysis (error inventory, patterns, pain points)
3. Proposed Result Pattern Design (type definitions, examples)
4. Method-by-Method Migration Assessment (table format)
5. Detailed Code Examples (before/after comparisons)
6. Breaking Changes & Impact Analysis
7. Phased Migration Strategy (step-by-step plan)
8. Testing & Validation Approach
9. Appendix: Alternative Patterns Considered

Use tables, code blocks with syntax highlighting, and clear section headers.
</output>

<verification>
Before declaring complete, verify your analysis:
- All 5 specified files have been examined
- At least 10 specific methods analyzed for Result pattern suitability
- Custom Result<T> implementation includes type definition with example usage
- Migration strategy includes at least 3 distinct phases
- Code examples compile conceptually (no syntax errors in examples)
- Breaking changes are explicitly called out with mitigation strategies
- Analysis addresses both Infrastructure and Application layers
- Recommendations are specific and actionable (not vague suggestions)
</verification>

<success_criteria>
- Complete analysis document saved to ./analyses/result-pattern-migration-plan.md
- Clear recommendation on whether to proceed with Result pattern migration
- Custom Result<T> design is production-ready
- Migration plan is detailed enough to begin implementation immediately
- Code examples demonstrate real transformations from current codebase
- All trade-offs and risks are explicitly documented
- Strategy accounts for incremental rollout without breaking changes
</success_criteria>
