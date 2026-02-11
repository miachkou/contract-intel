#!/usr/bin/env node

/**
 * PR Review Helper for Contract Intel
 * 
 * This script helps reviewers and PR authors by:
 * - Analyzing changed files for common issues
 * - Generating PR descriptions
 * - Suggesting conventional commit messages
 */

import { program } from 'commander';
import { execSync } from 'child_process';
import { readFileSync, existsSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Move to repo root
const repoRoot = join(__dirname, '..');
process.chdir(repoRoot);

/**
 * Get list of changed files in current branch vs main
 */
function getChangedFiles(base = 'origin/main') {
    try {
        const output = execSync(`git diff --name-only ${base}...HEAD`, { encoding: 'utf-8' });
        return output.trim().split('\n').filter(f => f);
    } catch (error) {
        console.warn(`Warning: Could not get diff against ${base}. Using staged files instead.`);
        try {
            const output = execSync('git diff --name-only --cached', { encoding: 'utf-8' });
            return output.trim().split('\n').filter(f => f);
        } catch (err) {
            return [];
        }
    }
}

/**
 * Get diff content for changed files
 */
function getDiff(base = 'origin/main') {
    try {
        return execSync(`git diff ${base}...HEAD`, { encoding: 'utf-8', maxBuffer: 10 * 1024 * 1024 });
    } catch (error) {
        try {
            return execSync('git diff --cached', { encoding: 'utf-8', maxBuffer: 10 * 1024 * 1024 });
        } catch (err) {
            return '';
        }
    }
}

/**
 * Analyze code for common issues
 */
function analyzeCode(files, diff) {
    const issues = {
        nullSafety: [],
        errorHandling: [],
        asyncPatterns: [],
        performance: [],
        codeSmells: [],
        security: []
    };

    // Backend analysis (C#)
    const csFiles = files.filter(f => f.endsWith('.cs'));
    if (csFiles.length > 0) {
        analyzeCSharpFiles(csFiles, diff, issues);
    }

    // Frontend analysis (TypeScript/React)
    const tsFiles = files.filter(f => f.endsWith('.ts') || f.endsWith('.tsx'));
    if (tsFiles.length > 0) {
        analyzeTypeScriptFiles(tsFiles, diff, issues);
    }

    return issues;
}

/**
 * Analyze C# files
 * Note: These are simple heuristics to catch common issues. May produce false positives.
 * Use as a starting point for review, not definitive judgments.
 */
function analyzeCSharpFiles(files, diff, issues) {
    const diffLines = diff.split('\n');
    const addedLines = diffLines.filter(line => line.startsWith('+')).join('\n');

    // Check for .Result or .Wait() usage (blocking async)
    if (addedLines.match(/\.Result\b/) || addedLines.match(/\.Wait\s*\(/)) {
        issues.asyncPatterns.push('⚠️  Consider using async/await instead of .Result or .Wait() to avoid blocking');
    }

    // Check for missing using statements with IDisposable
    if (addedLines.match(/new\s+\w*(Stream|Reader|Writer|Connection|Command)\w*/)) {
        issues.performance.push('ℹ️  Verify IDisposable objects use using statements or are properly disposed');
    }

    // Check for bare catch blocks (simplified check)
    if (addedLines.match(/catch\s*\(\s*Exception[^)]*\)/)) {
        issues.errorHandling.push('ℹ️  Review Exception handling - consider catching specific exception types');
    }

    // Check for potential N+1 query patterns in foreach loops
    if (addedLines.match(/foreach/) && addedLines.match(/\.(First|Single|Where|Count|Any)\(/)) {
        issues.performance.push('ℹ️  Review database queries in loops - watch for N+1 patterns');
    }

    // Check for magic numbers (numbers 10+, excluding common values)
    const magicNumbers = addedLines.match(/\b(?!0\b|1\b|10\b|100\b|1000\b)\d{2,}\b/g);
    if (magicNumbers && magicNumbers.length > 3) {
        issues.codeSmells.push('ℹ️  Consider extracting numeric constants to named values');
    }
}

/**
 * Analyze TypeScript/React files
 * Note: These are simple heuristics to catch common issues. May produce false positives.
 * Use as a starting point for review, not definitive judgments.
 */
function analyzeTypeScriptFiles(files, diff, issues) {
    const diffLines = diff.split('\n');
    const addedLines = diffLines.filter(line => line.startsWith('+')).join('\n');

    // Check for direct fetch instead of using api client
    if (addedLines.match(/fetch\s*\(/)) {
        issues.codeSmells.push('⚠️  Consider using the api.ts client instead of direct fetch calls');
    }

    // Check for any type usage
    if (addedLines.match(/:\s*any\b/)) {
        issues.codeSmells.push('⚠️  Avoid using "any" type - define proper types');
    }

    // Check for console.log (should be removed before merge)
    if (addedLines.match(/console\.log\(/)) {
        issues.codeSmells.push('ℹ️  Remove console.log statements before merging');
    }

    // Check for setState pattern that might be in loops
    if (addedLines.match(/\bset[A-Z]\w+\(/) && (addedLines.match(/\bfor\s*\(/) || addedLines.match(/\.forEach\(/))) {
        issues.performance.push('ℹ️  Review state updates - avoid calling setState multiple times in loops');
    }

    // Check for useEffect without considering cleanup
    if (addedLines.match(/useEffect\s*\(/)) {
        issues.performance.push('ℹ️  Review useEffect - ensure cleanup function if subscribing to events, timers, or connections');
    }

    // Check for missing error handling in async
    if (addedLines.match(/await\s+/) && !addedLines.includes('try')) {
        issues.errorHandling.push('ℹ️  Review async error handling - ensure errors are caught or propagated appropriately');
    }
}

/**
 * Categorize changed files
 */
function categorizeFiles(files) {
    const categories = {
        backend: files.filter(f => f.startsWith('src/') && f.endsWith('.cs')),
        frontend: files.filter(f => f.startsWith('webapp/') && (f.endsWith('.ts') || f.endsWith('.tsx'))),
        tests: files.filter(f => f.startsWith('tests/') || f.includes('.test.') || f.includes('.spec.')),
        docs: files.filter(f => f.endsWith('.md')),
        config: files.filter(f => f.match(/\.(json|yml|yaml|config|csproj)$/)),
        other: []
    };

    const categorized = new Set([
        ...categories.backend,
        ...categories.frontend,
        ...categories.tests,
        ...categories.docs,
        ...categories.config
    ]);

    categories.other = files.filter(f => !categorized.has(f));

    return categories;
}

/**
 * Determine the primary scope of changes
 */
function determineScope(categories) {
    if (categories.backend.length > categories.frontend.length) {
        if (categories.backend.some(f => f.includes('Controller'))) return 'api';
        if (categories.backend.some(f => f.includes('Domain'))) return 'domain';
        if (categories.backend.some(f => f.includes('Infrastructure'))) return 'infra';
        return 'backend';
    } else if (categories.frontend.length > 0) {
        return 'frontend';
    } else if (categories.tests.length > 0) {
        return 'test';
    } else if (categories.docs.length > 0) {
        return 'docs';
    }
    return null;
}

/**
 * Suggest conventional commit type based on changes
 */
function suggestCommitType(diff, files) {
    const diffLower = diff.toLowerCase();
    
    // Check for test files
    if (files.some(f => f.includes('test')) && !files.some(f => !f.includes('test') && f.endsWith('.cs'))) {
        return 'test';
    }
    
    // Check for documentation
    if (files.every(f => f.endsWith('.md'))) {
        return 'docs';
    }
    
    // Check for config/build changes
    if (files.every(f => f.match(/\.(json|yml|yaml|config|csproj)$/))) {
        return 'chore';
    }
    
    // Check for new features (new classes, methods, components)
    if (diffLower.includes('public class') || diffLower.includes('export function') || diffLower.includes('export const')) {
        return 'feat';
    }
    
    // Check for bug fixes
    if (diffLower.includes('fix') || diffLower.includes('bug') || diffLower.includes('issue')) {
        return 'fix';
    }
    
    // Check for refactoring
    if (diffLower.includes('refactor') || (diff.split('\n').filter(l => l.startsWith('-')).length > 10)) {
        return 'refactor';
    }
    
    return 'feat'; // default
}

/**
 * Generate commit message suggestion
 */
function generateCommitMessage(files, diff) {
    const categories = categorizeFiles(files);
    const type = suggestCommitType(diff, files);
    const scope = determineScope(categories);
    
    let description = '';
    
    if (categories.backend.length > 0) {
        const controllers = categories.backend.filter(f => f.includes('Controller'));
        const services = categories.backend.filter(f => f.includes('Service'));
        
        if (controllers.length > 0) {
            description = 'add/update API endpoints';
        } else if (services.length > 0) {
            description = 'update service logic';
        } else {
            description = 'update backend code';
        }
    } else if (categories.frontend.length > 0) {
        const pages = categories.frontend.filter(f => f.includes('Page'));
        const components = categories.frontend.filter(f => f.includes('component'));
        
        if (pages.length > 0) {
            description = 'update UI pages';
        } else if (components.length > 0) {
            description = 'update components';
        } else {
            description = 'update frontend code';
        }
    } else if (categories.docs.length > 0) {
        description = 'update documentation';
    } else if (categories.tests.length > 0) {
        description = 'add/update tests';
    }
    
    const scopePart = scope ? `(${scope})` : '';
    return `${type}${scopePart}: ${description}`;
}

/**
 * Generate PR description
 */
function generatePRDescription(files, diff, issues) {
    const categories = categorizeFiles(files);
    
    let output = '## Summary\n';
    output += '<!-- TODO: Add a brief description of what this PR does -->\n\n';
    
    output += '## Changes\n';
    if (categories.backend.length > 0) {
        output += '### Backend\n';
        categories.backend.forEach(f => output += `- ${f}\n`);
        output += '\n';
    }
    if (categories.frontend.length > 0) {
        output += '### Frontend\n';
        categories.frontend.forEach(f => output += `- ${f}\n`);
        output += '\n';
    }
    if (categories.tests.length > 0) {
        output += '### Tests\n';
        categories.tests.forEach(f => output += `- ${f}\n`);
        output += '\n';
    }
    if (categories.docs.length > 0) {
        output += '### Documentation\n';
        categories.docs.forEach(f => output += `- ${f}\n`);
        output += '\n';
    }
    if (categories.config.length > 0) {
        output += '### Configuration\n';
        categories.config.forEach(f => output += `- ${f}\n`);
        output += '\n';
    }
    
    output += '## Acceptance Criteria\n';
    output += '<!-- What should work after this PR is merged? -->\n';
    output += '- [ ] <!-- Add criteria here -->\n\n';
    
    output += '## Manual Testing Steps\n';
    output += '<!-- How to verify these changes work -->\n';
    output += '1. <!-- Add test steps here -->\n\n';
    
    output += '## Related Issues\n';
    output += '<!-- Link to related issues -->\n';
    output += 'Closes #\n\n';
    
    // Add review suggestions if there are issues
    const hasIssues = Object.values(issues).some(arr => arr.length > 0);
    if (hasIssues) {
        output += '## Review Notes\n';
        output += '<!-- Auto-generated suggestions from PR helper -->\n\n';
        
        if (issues.asyncPatterns.length > 0) {
            output += '**Async Patterns:**\n';
            issues.asyncPatterns.forEach(i => output += `${i}\n`);
            output += '\n';
        }
        
        if (issues.nullSafety.length > 0) {
            output += '**Null Safety:**\n';
            issues.nullSafety.forEach(i => output += `${i}\n`);
            output += '\n';
        }
        
        if (issues.errorHandling.length > 0) {
            output += '**Error Handling:**\n';
            issues.errorHandling.forEach(i => output += `${i}\n`);
            output += '\n';
        }
        
        if (issues.performance.length > 0) {
            output += '**Performance:**\n';
            issues.performance.forEach(i => output += `${i}\n`);
            output += '\n';
        }
        
        if (issues.codeSmells.length > 0) {
            output += '**Code Quality:**\n';
            issues.codeSmells.forEach(i => output += `${i}\n`);
            output += '\n';
        }
        
        if (issues.security.length > 0) {
            output += '**Security:**\n';
            issues.security.forEach(i => output += `${i}\n`);
            output += '\n';
        }
    }
    
    return output;
}

/**
 * Main command to generate PR description
 */
program
    .name('pr-review-helper')
    .description('PR Review Helper for Contract Intel')
    .version('1.0.0');

program
    .command('generate')
    .description('Generate PR description and commit message')
    .option('-b, --base <branch>', 'Base branch to compare against', 'origin/main')
    .action((options) => {
        console.log('🔍 Analyzing changes...\n');
        
        const files = getChangedFiles(options.base);
        
        if (files.length === 0) {
            console.log('No changes detected. Make sure you have committed changes or are on a branch with changes.');
            return;
        }
        
        console.log(`Found ${files.length} changed file(s)\n`);
        
        const diff = getDiff(options.base);
        const issues = analyzeCode(files, diff);
        
        // Generate commit message
        const commitMsg = generateCommitMessage(files, diff);
        console.log('📝 Suggested Commit Message:');
        console.log('─'.repeat(50));
        console.log(commitMsg);
        console.log('─'.repeat(50));
        console.log();
        
        // Generate PR description
        console.log('📋 PR Description:');
        console.log('═'.repeat(50));
        const prDescription = generatePRDescription(files, diff, issues);
        console.log(prDescription);
        console.log('═'.repeat(50));
        console.log();
        
        // Show analysis summary
        const totalIssues = Object.values(issues).reduce((sum, arr) => sum + arr.length, 0);
        if (totalIssues > 0) {
            console.log(`⚠️  Found ${totalIssues} suggestion(s) - review the "Review Notes" section above`);
        } else {
            console.log('✅ No obvious issues detected');
        }
        
        console.log('\n💡 Tip: Review the full PR_REVIEW_GUIDE.md for comprehensive review guidelines');
    });

program
    .command('review')
    .description('Quick review checklist for current changes')
    .option('-b, --base <branch>', 'Base branch to compare against', 'origin/main')
    .action((options) => {
        console.log('📋 Quick Review Checklist\n');
        
        const files = getChangedFiles(options.base);
        const diff = getDiff(options.base);
        const issues = analyzeCode(files, diff);
        
        console.log('Files Changed:', files.length);
        console.log('─'.repeat(50));
        
        const categories = categorizeFiles(files);
        if (categories.backend.length > 0) console.log(`  Backend: ${categories.backend.length} files`);
        if (categories.frontend.length > 0) console.log(`  Frontend: ${categories.frontend.length} files`);
        if (categories.tests.length > 0) console.log(`  Tests: ${categories.tests.length} files`);
        if (categories.docs.length > 0) console.log(`  Docs: ${categories.docs.length} files`);
        console.log();
        
        console.log('Code Analysis:');
        console.log('─'.repeat(50));
        
        let hasIssues = false;
        
        if (issues.asyncPatterns.length > 0) {
            console.log('\n⚠️  Async Patterns:');
            issues.asyncPatterns.forEach(i => console.log(`  ${i}`));
            hasIssues = true;
        }
        
        if (issues.nullSafety.length > 0) {
            console.log('\nℹ️  Null Safety:');
            issues.nullSafety.forEach(i => console.log(`  ${i}`));
            hasIssues = true;
        }
        
        if (issues.errorHandling.length > 0) {
            console.log('\nℹ️  Error Handling:');
            issues.errorHandling.forEach(i => console.log(`  ${i}`));
            hasIssues = true;
        }
        
        if (issues.performance.length > 0) {
            console.log('\n⚠️  Performance:');
            issues.performance.forEach(i => console.log(`  ${i}`));
            hasIssues = true;
        }
        
        if (issues.codeSmells.length > 0) {
            console.log('\n⚠️  Code Quality:');
            issues.codeSmells.forEach(i => console.log(`  ${i}`));
            hasIssues = true;
        }
        
        if (issues.security.length > 0) {
            console.log('\n🔒 Security:');
            issues.security.forEach(i => console.log(`  ${i}`));
            hasIssues = true;
        }
        
        if (!hasIssues) {
            console.log('✅ No obvious issues detected');
        }
        
        console.log('\n\n📖 Manual Checklist:');
        console.log('─'.repeat(50));
        console.log('  [ ] Tests pass (dotnet test)');
        console.log('  [ ] Code builds (dotnet build)');
        console.log('  [ ] Frontend builds (cd webapp && npm run build)');
        console.log('  [ ] Changes reviewed against PR_REVIEW_GUIDE.md');
        console.log('  [ ] PR description is complete');
        console.log('  [ ] Commit message follows conventional commits');
        console.log();
    });

program.parse();
