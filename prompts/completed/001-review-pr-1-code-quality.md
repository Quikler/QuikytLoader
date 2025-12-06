<objective>
Conduct a comprehensive code quality and architecture review of Pull Request #1 using the GitHub CLI (gh) tool.

This review will help ensure the PR meets quality standards before merging and provide actionable feedback to improve the codebase.
</objective>

<context>
You are reviewing Pull Request #1 in the QuikytLoader repository, an Avalonia UI desktop application following MVVM architecture with clean separation of concerns.

Read the CLAUDE.md file to understand project conventions, architecture patterns, and coding standards that should be followed.
</context>

<requirements>
1. Use the `gh` CLI tool to fetch PR #1 details, including:
   - PR description and metadata
   - ALL PR review comments (especially from @claude)
   - Changed files list
   - Full diff of all changes

2. Analyze existing @claude comments on the PR:
   - Evaluate whether each comment is valid and makes sense
   - Check if the concerns raised are legitimate based on project standards
   - Identify any comments that may be incorrect, overly pedantic, or not aligned with CLAUDE.md
   - Note which comments provide valuable feedback vs which should be disregarded
   - Assess if @claude's suggestions follow best practices and project conventions

3. Analyze the code changes for:
   - **Architecture**: Adherence to MVVM pattern, separation of concerns, dependency injection usage
   - **Design patterns**: Single Responsibility Principle, Law of Demeter, proper use of commands/observables
   - **Code organization**: File structure, naming conventions, method/class sizing
   - **Best practices**: Error handling, async/await patterns, resource disposal, null safety
   - **Maintainability**: Code clarity, unnecessary complexity, potential refactoring opportunities
   - **Consistency**: Alignment with existing codebase patterns documented in CLAUDE.md

4. For each finding, provide:
   - File path and line number reference (file_path:line_number format)
   - Severity level (Critical/Major/Minor/Suggestion)
   - Clear description of the issue
   - Specific recommendation for improvement
   - Code example if applicable

5. Generate a comprehensive review report with all requested outputs
</requirements>

<implementation>
1. Start by reading CLAUDE.md to understand project standards
2. Use `gh pr view 1 --json title,body,state,author,files,additions,deletions` to get PR metadata
3. Use `gh pr view 1 --comments` or `gh api repos/:owner/:repo/pulls/1/comments` to fetch all PR review comments
4. Use `gh pr diff 1` to get the full diff
5. First, analyze all @claude comments and assess their validity against project standards
6. Then systematically review each changed file against the architecture and quality criteria
7. Organize findings by file, then by severity within each file
</implementation>

<output>
Create a comprehensive review document saved to: `./pr-reviews/pr-1-review.md`

The review must include:

### 1. Executive Summary
- PR title and author
- Overall assessment (Approve/Request Changes/Reject)
- Key statistics (files changed, lines added/removed)
- High-level summary of changes

### 2. Analysis of @claude's Comments
For each comment from @claude on the PR:
- Quote the original comment
- File and line reference
- **Validity Assessment**: Valid / Questionable / Invalid
- **Reasoning**: Why the comment makes sense or doesn't, based on project standards
- **Recommendation**: Keep as-is / Modify / Disregard

### 3. Architecture & Design Analysis
- How well changes follow MVVM pattern
- Dependency injection usage
- Separation of concerns assessment
- Design pattern adherence

### 4. Detailed Findings
For each file with issues:
```markdown
#### path/to/file.cs
- **[Severity]** (line X): Description
  - **Issue**: What's wrong and why it matters
  - **Recommendation**: Specific improvement suggestion
  - **Example**: Code snippet if helpful
```

### 5. Code Improvement Suggestions
Broader refactoring opportunities or enhancements beyond specific issues

### 6. GitHub-Ready Comments
Format findings as ready-to-paste GitHub review comments:
```markdown
**File: path/to/file.cs, Line X**
[Severity] Description

Recommendation: ...

```suggestion
// suggested code change
```
```

### 7. Final Recommendation
Clear approve/request changes/reject decision with justification, including:
- Summary of @claude's comment validity (how many were valid vs questionable/invalid)
- Whether the PR should be approved based on both your review AND the analysis of @claude's feedback
</output>

<verification>
Before declaring complete, verify:
- All @claude comments have been analyzed for validity
- All changed files in the PR have been reviewed
- Each finding includes file path, line number, severity, and recommendation
- Review document includes all 7 required sections
- GitHub comment format is properly structured for easy copy-paste
- Final recommendation is clearly stated with reasoning
- Assessment of @claude's comments is included in final recommendation
</verification>

<success_criteria>
- Complete review document saved to ./pr-reviews/pr-1-review.md
- All @claude comments analyzed with validity assessment
- All architecture and code quality aspects analyzed
- Findings are specific with file:line references
- Output includes both detailed analysis and GitHub-ready comments
- Clear approval/rejection recommendation with justification
- Meta-analysis of @claude's review quality included
</success_criteria>
