## Tester Details
**Tester Name**: [Your Name]  
**Chains Tested**: [List of chains]  
**Platforms Tested**: [List of platforms]  
**Wallets Used**: [List of wallets]

---

## Unity Test Plan (1.3.0) - Futureverse
Test Level: Assets/Samples/Emergence SDK/.../Core Samples/Scenes

### Pre-requisites for importing the project

"Can successfully add package from git URL: Window > Package Manager > + > Add package from git URL...
https://github.com/CrucibleNetworksLtd/EmergenceSDKUnity.git#InternalQA"																									
Can successfully import Futureverse Samples from Emergence SDK package in the Package Manager window.																									

### Automated Tests

- [ ] Can access Window > General > Test Runner																									
- [ ] Can run all 5 automated test categories under ... > Futureverse																									

### 1. Player Mechanics

**Test Description**:
Core user - engine player mechanics involving movement and base key commands.

**Expected Output**:
- [ ] The user can move around freely using WASD, SPACE and SHIFT.																									
- [ ] The user can look around using the mouse.																									
- [ ] The user can click ESC to enable the mouse cursor.																									

### 2. Wallet System

#### 2.1. FTUE UI 

**Test Description**:
Basic UI functionality. Makes sure the engine loads the Emergence login sequence UI correctly.

**Expected Output**:
- [ ] Pressing E on the OPENING OVERLAY panel will open the (FTUE) UI overlay.																									
- [ ] Pressing SPACE while the UI is open will advance to the next UI page.																									
- [ ] Pressing ESC while the UI is open will close the UI.																									
- [ ] Manually clicking on PRESS ESC TO QUIT button at the top left on the second UI page will close the UI.																									

#### 2.2. Login Screen 

**Test Description**:
Login sequence functionality. Makes sure the user can log in to Emergence.

**Required Inputs**:
- Default Login Flow MUST be set to Futurepass. 
Go to Hierarchy > Futurepass Emergence Demo Scene > Emergence (singleton) > Inspector > Settings for the default Emergence UI > Default Login Flow, set to Futurepass.

**Expected Output**:
- [ ] Changing the QR Code Timer time to a different number will force the QR Code to refresh at a different time in seconds.
Go to Hierarchy > Futurepass Emergence Demo Scene > Emergence (singleton) > Inspector > Settings for the default Emergence UI > QR Code Timeout.
- [ ] There is a QR Code that can be scanned with a mobile wallet app.
- [ ] There is a wc address that can be selected and copied below the QR Code.
- [ ] There is Copy button to the right of the wc address button that can be pressed and will copy to clipboard the address.
- [ ] There is a counter below the QR Code Timer that counts to zero each second.
- [ ] Once the timer reaches zero, the QR Code and wc address will reset.
- [ ] If the user declines the sign in request the QR code is refreshed.
- [ ] The user can scan the QR code with their wallet app and get a sign in request.
- [ ] The user can sign the request and be logged in to their Emergence account.

#### Unit 2.2.1. Logging in with an Existing Futurepass wallet

**Expected Output**:
- [ ] The user can sign the request and be logged in with their Futurepass account.

#### Unit 2.2.2. Logging in without a Futurepass-registered wallet

**Expected Output**:
- [ ] The user scans the QR Code, signs the request, and is shown two buttons: Create Futurepass and Retry
"- [ ] Create Futurepass links the user to the correct website (depending on build environment): 
For Production: https://futurepass.futureverse.app/
For Dev/Staging: https://identity-dashboard.futureverse.cloud/"
- [ ] Retry button returns the user to the QR Code screen.

#### 2.3. Send ARTM

**Test Description**:
Makes sure the user is able to send ARTM, sign it, and Emergence to confirm it.

**Expected Output**:
- [ ] Pressing E on the button of SEND ARTM panel will send a message request to the wallet app.
- [ ] The user can SIGN the message request in the wallet app.
- [ ] At the top right, "The test is still successful" should show if the Send ARTM test is successful.

#### 2.4. Inventory Service

**Test Description**:
Makes sure the user is able to view their NFTs from their Futurepass inventory.

**Expected Output**:
- [ ] Pressing E on the button of the INVENTORY SERVICE panel will bring up a white ribbon at the bottom of the screen which shows a thumbnail and name of NFTs present on the wallet used to log in.
If there are no NFTs in the wallet, no items will show.
- [ ] Pressing E on the button again will bring the white ribbon down.

### 3. Persona System

**Test Description**:
Makes sure the Persona System works correctly.

#### Unit 3.1. Basic functionality of the dashboard screen

**Required Inputs**:
- The user has at least four personas.

**Expected Output**:
- [ ] When the UI is first opened the current selected persona is the active persona.
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
- [ ] The hexagon in the positions immediately to the left and right of the active persona:
- [ ] Are a bit smaller than the central hexagon.
- [ ] Have a small amount of blur.
- [ ] The hexagon in the positions two to the left and right of the active persona:
- [ ] Are much smaller than the central hexagon.
- [ ] Have a medium amount of blur.

#### Unit 3.3. Correct display of the persona information

**Expected Output**:
- [ ] Each hexagon has the correct icon
- [ ] Each hexagon has the correct name
- [ ] Each hexagon has the correct description

#### Unit 3.4. Create persona screen basic functionality

**Expected Output**:
- [ ] The user can see a plus in the top left corner.
- [ ] Pressing the plus:
- [ ] Takes the user to the avatar selection screen.
- [ ] The user can see a back button in the bottom left corner.
- [ ] The user can see a persona information button in the bottom right corner.
- [ ] Pressing the back button takes the user back to the dashboard screen, with no new persona created.
- [ ] Using the flow in 3.5., the user can select an avatar.
- [ ] Pressing the persona information button:
- [ ] Takes the user to the persona information screen. (see unit 3.6. for functionality here)
- [ ] The user can see a select avatar button in the bottom left corner.
- [ ] Pressing the select avatar button takes the user to the avatar selection screen.
- [ ] The user can see a create persona button in the bottom right corner.
- [ ] The stages UI is showing 2 of 3.
- [ ] Using flow in 3.6., the user can create a persona.
- [ ] Pressing the create persona button:
- [ ] Takes the user to the dashboard screen.
- [ ] The user can see a new persona in the dashboard screen.
- [ ] The new persona is the active persona.
- [ ] The avatar icon is the avatar that was selected in the avatar selection screen.
- [ ] The name is the name that was entered in the persona information screen.
- [ ] The description is the description that was entered in the persona information screen.
- [ ] The users avatar model will change to the avatar that was selected in the avatar selection screen (this may take up to 30s to change).

#### Unit 3.5. Avatar selection screen basic functionality

**Expected Output**:
- [ ] There should be an icon for each avatar arranged in a grid on the left.
- [ ] The user can see the icon of the selected avatar on the right.
- [ ] The user can see the icon of the selected avatar on the right blurred and scaled behind the avatar.
- [ ] The user can see a [back/cancel] button in the bottom left corner.
- [ ] The user can see a [next button/confirm avatar] in the bottom right corner.
- [ ] Pressing the [back/cancel] button if the selected avatar was different to the active avatar, the active avatar is not changed.
- [ ] Pressing the [next button/confirm avatar] button if the selected avatar was different to the active avatar, the active avatar is changed to the selected avatar.

#### Unit 3.6. Persona information screen basic functionality

**Expected Output**:
- [ ] The user can see a text box for the name.
- [ ] The user can see a text box for the description.
- [ ] If the user tries to proceed without entering a name of at least 3 characters, the user is shown an error message.
- [ ] The user can see the icon of the current avatar on the right.
- [ ] The user can see the icon of the current avatar on the right blurred and scaled behind the avatar.
- [ ] When the user proceeds, the name and description are saved to the persona.
- [ ] When the user proceeds, the users avatar is set to the one displayed on the right.

#### Unit 3.7. Edit persona screen basic functionality

**Required Inputs**:
- The user has at least two avatars.

- [ ] The user can see the name and description of the current persona in the box under name.
- [ ] The user can read the bio in the box under bio.
- [ ] The user can see a button saying replace avatar under the icon.
- [ ] The user can see a back button in the bottom left corner.
- [ ] Pressing the back button does not save any changes made to the persona.
- [ ] The user can see a save changes button in the bottom right corner.
- [ ] Pressing the save changes button the changes made to the persona are saved.
- [ ] If the persona is not the active persona:
- [ ] The user can see a delete persona button in the bottom right corner.
- [ ] Pressing the delete persona button:
- [ ] Takes the user back to the dashboard screen.
- [ ] The persona is deleted.
- [ ] If the persona is the active persona the user will see a greyed out delete trash icon.

#### Unit 3.8. Edit/Create persona screen advanced functionality - correct flow

**Expected Output**:
- [ ] The user is taken to the select avatar screen when pressing the top left plus button.
- [ ] There is a status widget in the top left corner that says 1 of 3 Create Avatar.
- [ ] The user is taken back to the dashboard when pressing back from this screen
- [ ] The user is taken to the persona information screen when pressing the bottom right button.
- [ ] The user is taken back to the avatar selection when pressing back from this screen
- [ ] There is a status widget in the top left corner that says 2 of 3 Create Persona.
- [ ] The user is taken back to the dashboard when pressing next from this screen
- [ ] The user is taken to the edit persona screen when pressing the edit button on the top right of the persona info widget
- [ ] There is a status widget in the top left corner that says EDIT PERSONA.
- [ ] The user is taken back to the dashboard screen when pressing cancel from this screen
- [ ] The user is taken back to the dashboard screen when pressing save changes from this screen
- [ ] The user is taken to the avatar selection screen when pressing replace avatar from this screen
- [ ] The user is taken to the persona information screen when pressing next from this screen
- [ ] The user is taken to the persona information screen when pressing cancel from this screen
- [ ] There is a status widget in the top left corner that says EDIT PERSONA > AVATAR SELECTION.
- [ ] The user is taken to the dashboard screen when pressing delete persona from this screen (The persona must not be active for delete to work)

#### Unit 3.9. Create persona screen advanced functionality - starting fresh

**Expected Output**:
- [ ] The user is taken to the create persona screen when pressing the central plus button.
- [ ] The user can complete the flow in 3.4.


---

This test plan is structured by user flow, with each component of the flow coming one after the other.

- **Test Description**: Explanation of [what is being tested and why].
- **Required Inputs**: Any input data needed for the test. [Input 1, Input 2, ...]
- **Expected Output**: Description of the expected result of the test. [Output description]

### Template Unit (Outdated)

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