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

**Priority**: Medium

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

#### Unit 1.1.4: Create persona screen basic functionality.

**Priority**: Medium

**Test Description**:
Making sure that the UI is displayed correctly and that the user can create a persona.

**Expected Output**:
- [ ] The user can see a plus in the top left corner.
- Pressing the plus:
  - [ ] Takes the user to the avatar selection screen.
  - [ ] Displays a UI illustration the stages of persona creation in the top right (3 is a check/tick).
  - [ ] The stages UI is showing 1 of 3.
  - [ ] The user can see a back button in the bottom left corner.
  - [ ] The user can see a persona information button in the bottom right corner.
  - [ ] Pressing the back button takes the user back to the dashboard screen, with no new persona created.
  - [ ] Using the flow in 1.1.5, the user can select an avatar.
- Pressing the persona information button:
  - [ ] Takes the user to the persona information screen. (see unit 1.1.6 for functionality here)
  - [ ] The user can see a select avatar button in the bottom left corner.
  - [ ] The user can see a create persona button in the bottom right corner.
  - [ ] The stages UI is showing 2 of 3.
  - [ ] Using flow in 1.1.6, the user can create a persona.
- Pressing the create persona button:
  - [ ] Takes the user to the dashboard screen.
  - [ ] The user can see a new persona in the dashboard screen.
  - [ ] The new persona is the active persona.
  - [ ] The avatar icon is the avatar that was selected in the avatar selection screen.
  - [ ] The name is the name that was entered in the persona information screen.
  - [ ] The description is the description that was entered in the persona information screen.
  - [ ] The users avatar model will change to the avatar that was selected in the avatar selection screen (this may take up to 30s to change).

#### Unit 1.1.5: Avatar selection screen basic functionality.

**Priority**: Medium

**Test Description**:
Making sure that the UI is displayed correctly and that the user can select an avatar.

**Glossary**
- Active Avatar: The avatar that the user has currently set to active in Emergence.
- Selected Avatar: The avatar that the user has selected to view in the edit persona screen, it may not be the same as the active avatar.

**Expected Output**:
- [ ] There should be an icon for each avatar arranged in a grid on the left.
- [ ] The user can see the icon of the selected avatar on the right.
- [ ] The user can see the icon of the selected avatar on the right blurred and scaled behind the avatar.
- [ ] The user can see a cancel button in the bottom left corner.
- [ ] The user can see a confirm avatar button in the bottom right corner.
- [ ] Pressing the cancel button:
  - [ ] Takes the user back to the create persona screen.
  - [ ] If the selected avatar was different to the active avatar, the active avatar is not changed.
- Pressing the confirm avatar button:
  - [ ] Takes the user back to the create persona screen.
  - [ ] If the selected avatar was different to the active avatar, the active avatar is changed to the selected avatar.
  - [ ] If the selected avatar was the same as the active avatar, the active avatar is not changed.


#### Unit 1.1.6: Edit persona screen basic functionality.

**Priority**: Medium

**Test Description**:
Making sure that the UI is displayed correctly and that the user can edit the persona information.

**Glossary**
- Active Persona: The persona that the user has currently set to active in Emergence.
- Active Avatar: The avatar that the user has currently set to active in Emergence.
- Selected Avatar: The avatar that the user has selected to view in the edit persona screen, it may not be the same as the active avatar.

**Dependencies**:
- Unit 1.1.4, as you need to be able to create a persona to edit it.

**Required Inputs**:
- The user has at least two avatars.

**Expected Output**:
- [ ] The user can see the name and description of the current persona in the box under name.
- [ ] The user can read the bio in the box under bio.
- [ ] The user can see the icon of the current avatar on the right.
- [ ] The user can see the icon of the current avatar on the right blurred and scaled behind the avatar.
- [ ] The user can see a button saying replace avatar under the icon.
- [ ] The user can see a back button in the bottom left corner.
- [ ] Pressing the back button:
  - [ ] Takes the user back to the dashboard screen.
  - [ ] Does not save any changes made to the persona.
- [ ] The user can see a save changes button in the bottom right corner.
- [ ] Pressing the save changes button:
  - [ ] Takes the user back to the dashboard screen.
  - [ ] The changes made to the persona are saved.
- [ ] If the persona is not the active persona: 
  - [ ] The user can see a delete persona button in the bottom right corner.
  - [ ] Pressing the delete persona button:
    - [ ] Takes the user back to the dashboard screen.
    - [ ] The persona is deleted.
- [ ] If the persona is the active persona the user cannot see the delete button.

#### Unit 1.1.7: Create persona screen advanced functionality - starting fresh.

**Priority**: Low

**Test Description**:
Testing that the user can create a persona from scratch.

**TO DO**

---

## Featureset 2 Wallet System

### Feature 2.1 Login Screen

This is the screen that the user sees when they first open the app. It allows them to login to their Emergence account by scanning a QR code with their wallet app.

#### Unit 2.1.1: Basic functionality of the login screen.

**Priority**: High

**Test Description**:
Making sure that the UI is displayed correctly and that the user can login to their Emergence account.

**Expected Output**:
- [ ] The user can see a QR code on the screen when they open the overlay.
- [ ] The user can scan the QR code with their wallet app and get a sign in request.
- [ ] The user can sign the request and be logged in to their Emergence account.
- [ ] If the user declines the sign in request the QR code is refreshed.
- [ ] The QR code is refreshed every 60 seconds.
- [ ] The QR code time to refresh is displayed on the screen under the QR code.


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