# 7Bridges - Educational Graph Theory Game

A Unity educational simulation game teaching graph theory concepts through interactive exploration of the Königsberg bridges problem.

**Target Audience:** Year 10 mathematics students  
**Platform:** Desktop PC (Windows/Mac)  
**Distribution:** itch.io  

## 🚨 Critical Issues - Sprint 3 Priority - August 23

### Console Log Analysis Results
Based on recent console logs, the following critical bugs have been identified:

**Mission State Management:**
- [ ] **CRITICAL** - Journey state persists between missions causing instant completions
- [ ] **CRITICAL** - `ResetJourney()` method not properly clearing state
- [ ] **CRITICAL** - Mission 3 completes immediately with no player input

**Vertex-Bridge Coordination:**
- [ ] **CRITICAL** - Missing vertex detection for Bridge F and Bridge D
- [ ] **HIGH** - Players cross bridges without triggering vertex events
- [ ] **HIGH** - Incomplete vertex-bridge endpoint connections

## 📋 Sprint 3 Task Checklist

### Epic 1: Journey State Management
- [ ] **Task 1: Fix Mission Reset Logic** (5 SP)
  - [ ] Add debug logging to `ResetJourney()` method
  - [ ] Verify `currentJourney.Clear()` execution
  - [ ] Ensure `startingVertex = null` assignment works
  - [ ] Test mission transitions with clean state
  - [ ] **AC:** All missions start with confirmed clean state

- [ ] **Task 2: Debug Journey Analysis Logic** (3 SP)
  - [ ] Add logging to `AnalyzeCurrentJourney()` method
  - [ ] Log mission completion checks in `IsMissionComplete()`
  - [ ] Create test cases for path/trail/walk validation
  - [ ] **AC:** Journey classification logic verified with comprehensive logging

### Epic 2: Vertex-Bridge Coordination  
- [ ] **Task 3: Fix Missing Vertex Detection** (8 SP)
  - [ ] Audit all bridge-vertex connections
  - [ ] Verify vertex triggers at Bridge F endpoints
  - [ ] Verify vertex triggers at Bridge D endpoints  
  - [ ] Test trigger collider sizes and positioning
  - [ ] **AC:** Every bridge crossing logs vertex detection before/after

- [ ] **Task 4: Validate Network Topology** (3 SP)
  - [ ] Create visual debug mode for vertex-bridge connections
  - [ ] Document actual network structure (A-B-C-D relationships)
  - [ ] Fix any orphaned bridges or vertices
  - [ ] **AC:** Complete network topology documented and verified

### Epic 3: Mission Progression System
- [ ] **Task 5: Test Incorrect Journey Validation** (5 SP)
  - [ ] Test repeated vertices in path missions
  - [ ] Test repeated edges in trail missions
  - [ ] Verify corrective feedback appears
  - [ ] Ensure invalid journeys don't complete missions
  - [ ] **AC:** Educational feedback system works correctly

- [ ] **Task 6: Mission State Isolation** (3 SP)
  - [ ] Add unit tests for mission transition logic
  - [ ] Verify mission independence
  - [ ] **AC:** Previous mission data doesn't affect current validation

### Development Tasks
- [ ] **Task 7: Enhanced Debug Console** (2 SP)
  - [ ] Add timestamp to all journey events
  - [ ] Implement color-coded log levels
  - [ ] Create journey data export functionality
  - [ ] **AC:** Structured logging system implemented

- [ ] **Task 8: Font Asset Fixes** (1 SP)
  - [ ] Replace ✓ with text-based completion indicators
  - [ ] Replace 🎉 with text-based celebration message
  - [ ] **AC:** All UI text renders without Unicode warnings

## 🗂️ Repository Organization

### Current Folder Structure
```
Assets/
├── Scripts/
│   ├── 7Bridges/               # Core game systems
│   │   ├── Core/              # Bridge.cs, Vertex.cs, Player.cs
│   │   ├── Journey/           # JourneyTracker.cs, JourneyStep.cs
│   │   ├── Missions/          # LearningMissionsManager.cs
│   │   ├── UI/                # BridgeUIManager.cs
│   │   └── Validation/        # BridgeValidator.cs, IBridgeValidator.cs
│   ├── Controllers/           # ThirdPersonController.cs, StarterAssetsInputs.cs
│   └── Utilities/             # BasicRigidBodyPush.cs
├── Documentation/             # Links to Google Drive docs
└── ProjectManagement/         # Sprint planning, retrospectives
```

### Setup Checklist
- [ ] Organize scripts into recommended folder structure
- [ ] Create feature branches for each Sprint 3 task
- [ ] Set up GitHub Issues for all critical bugs
- [ ] Link Google Drive documentation in repository
- [ ] Configure automated project board workflows

## 📁 Documentation Strategy

### Google Drive Structure
```
7Bridges Project/
├── 01-Design Documents/       # GameDev Capstone presentation
├── 02-Technical Specs/        # Architecture diagrams
├── 03-Sprint Documentation/   # Sprint reviews, retrospectives  
├── 04-Educational Content/    # NSW syllabus alignment
├── 05-Testing & Validation/   # Console log analysis
└── 06-Deployment/            # Build configurations, itch.io prep
```

### Documentation Tasks
- [ ] Upload console log analysis to Google Drive
- [ ] Create technical specification document
- [ ] Document network topology (vertex-bridge relationships)
- [ ] Link all Drive documents in GitHub issues
- [ ] Create quick reference guide in GitHub Wiki

## 🔄 Daily Workflow

### Morning Routine
- [ ] Review GitHub Project board
- [ ] Pull latest changes from main branch
- [ ] Check overnight build/test results
- [ ] Review any new console log reports

### Development Cycle
- [ ] Work in feature branches with descriptive names
- [ ] Commit frequently with issue references
- [ ] Document any new bugs in Google Drive
- [ ] Create GitHub issues for blocking problems
- [ ] Test locally before pushing

### Evening Routine  
- [ ] Push all changes with descriptive commit messages
- [ ] Update GitHub Project board status
- [ ] Sync any new documentation to Google Drive
- [ ] Review tomorrow's priority tasks

## 🎯 Success Criteria

### Technical Goals
- [ ] All missions start with clean state (no persistence bugs)
- [ ] Complete vertex detection at all bridge endpoints
- [ ] Proper journey type classification (walk/trail/path/circuit/cycle)
- [ ] Educational feedback system provides meaningful guidance
- [ ] Zero critical console errors during gameplay

### Educational Goals
- [ ] Clear learning progression through NSW syllabus concepts
- [ ] Meaningful feedback for incorrect journeys
- [ ] Intuitive understanding of graph theory fundamentals
- [ ] Successful completion of all mission types

### Project Management Goals  
- [ ] Sprint 3 completed on schedule
- [ ] All critical bugs resolved
- [ ] Documentation updated and accessible
- [ ] Smooth workflow between GitHub and Google Drive
- [ ] Prepared for Sprint 4 development

## 📊 Current Status

**Console Log Issues Identified:** 3 Critical, 2 High Priority  
**Sprint 3 Tasks Created:** 8 Tasks, 30 Total Story Points  
**GitHub Project Board:** Ready for Sprint 3 execution  
**Documentation:** GameDev Capstone complete, technical specs in progress  

---

**Next Steps:** Begin with Task 1 (Mission Reset Logic) as it blocks all other mission functionality. Implement comprehensive logging before attempting fixes to ensure visibility into system behavior.