## Tester Details
**Tester Name**: [Your Name]  
**Chains Tested**: [List of chains]  
**Platforms Tested**: [List of platforms]  
**Wallets Used**: [List of wallets]

---

## Featureset 1 Persona System

### Feature 1.1 Dashboard Screen

This is the screen with the hexagon UI that shows the user's current persona and the personas they can switch to.

#### Unit 1.1.1: Basic functionality of the dashboard screen.

**Priority**: High

**Test Description**:
Basic functionality of the dashboard screen. Making sure that the UI is displayed correctly and that the user can view all the correct persona information.

**Glossary**
- Active Persona: The persona that the user has currently set to active in Emergence.
- Selected Persona: The persona that the user has selected to view in the dashboard screen, it may not be the same as the active persona.

**Required Inputs**:
- The user has at least four personas.

**Expected Output**:
- [ ] There is a hexagon for each persona.
- [ ] The current persona is highlighted by a green light in the top corner of that hexagon.
- [ ] No other persona has a green light.
- [ ] Clicking on a persona moves that one into the center, and its neighbours move to accomodate.
- [ ] The user can see the name and description of the current persona in the box below the icon.
- [ ] Clicking the right arrow moves the hexagons one space to the right.
- [ ] Clicking the left arrow moves the hexagons one space to the left.


#### Unit 1.1.2: Intermediate functionality of the dashboard screen.

**Priority**: Medium

**Test Description**:
Basic functionality of the dashboard screen. Making sure that the UI is displayed correctly and that the user can view all the correct persona information.

**Glossary**
- Active Persona: The persona that the user has currently set to active in Emergence.
- Selected Persona: The persona that the user has selected to view in the dashboard screen, it may not be the same as the active persona.

**Required Inputs**:
- The user has at least four personas.

**Expected Output**:
- [ ] When the UI is first opened the current selected persona is the active persona.
- You should only see 2 personas either side of the current persona (It is important to have at least 4 personas for this check):
  - [ ] If you select the rightmost hexagon you shouldn't see the leftmost hexagon.
  - [ ] If you select the leftmost hexagon you shouldn't see the rightmost hexagon.
- [ ] If you select the second hexagon from the right you should see the leftmost hexagon.
- [ ] If you select the second hexagon from the left you should see the rightmost hexagon.
- [ ] Clicking the small hexagon in the top left corner of the screen takes you to the current active persona.
- [ ] Clicking the small plus below the small hexagon takes you to the create persona screen.
- [ ] The active persona has a badge in the description box that says "Active"
- [ ] A selected non-active persona has a button to activate it in the description box.
- [ ] Pressing the paper and pen icon takes you to the edit persona screen.
- [ ] The trash icon is greyed out on the active persona.
- [ ] Pressing the trash on a non-active icon deletes the persona.
- The hexagon in the positions immediately to the left and right of the active persona:
  - [ ] Are a bit smaller than the central hexagon.
  - [ ] Have a small amount of blur.
- The hexagon in the positions two to the left and right of the active persona:
  - [ ] Are a much smaller than the central hexagon.
  - [ ] Have a medium amount of blur.

#### Unit 1.1.3: Correct display of the persona information.

**Priority**: High

**Test Description**:
Making sure that the UI is displaying the correct persona information.

**Dependencies**:
- Unit 1.1.1, as you need to be able to navigate the UI to see the information.

**Expected Output**:
- [ ] Each hexagon has the correct icon
- [ ] Each hexagon has the correct name
- [ ] Each hexagon has the correct description

---




---

This test plan is structured by featuresets, each containing multiple features, and each feature containing multiple units. For each unit, the following information is provided:

- **Priority**: Level of importance ([High/Medium/Low]).
    - **High**: Should be tested if the change touched this featureset.
    - **Medium**: Should be tested if the change specifically altered this feature.
    - **Low**: Should be tested if the change touched this particular unit.

- **Test Description**: Explanation of [what is being tested and why].
- **Dependencies**: Any preconditions or tests that must be completed successfully before this test. [None or list dependencies]
- **Required Inputs**: Any input data needed for the test. [Input 1, Input 2, ...]
- **Expected Output**: Description of the expected result of the test. [Output description]

### Template Unit

#### Unit x.y.z: [Name]

**Priority**: [High/Medium/Low]

**Test Description**:
- [What we are testing and why.]

**Glossary**
[Optional glossary of terms used in this unit.]
- [Term 1]: [Definition] 

**Dependencies**:
- [Must complete Unit 1.1.1 successfully or other dependencies.]

**Required Inputs**:
- [Input 1]
- [Input 2]

**Expected Output**:
- [ ] [Output description]