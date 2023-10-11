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
  - [ ] The user can see a back button in the bottom left corner.
  - [ ] The user can see a persona information button in the bottom right corner.
  - [ ] Pressing the back button takes the user back to the dashboard screen, with no new persona created.
  - [ ] Using the flow in 1.1.5, the user can select an avatar.
- Pressing the persona information button:
  - [ ] Takes the user to the persona information screen. (see unit 1.1.6 for functionality here)
  - [ ] The user can see a select avatar button in the bottom left corner.
  - [ ] Pressing the select avatar button takes the user to the avatar selection screen.
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
- [.../...] The left option is for the create persona flow, the right option is for the edit persona flow.

**Expected Output**:
- [ ] There should be an icon for each avatar arranged in a grid on the left.
- [ ] The user can see the icon of the selected avatar on the right.
- [ ] The user can see the icon of the selected avatar on the right blurred and scaled behind the avatar.
- [ ] The user can see a [back/cancel] button in the bottom left corner.
- [ ] The user can see a [next button/confirm avatar] in the bottom right corner.
- [ ] Pressing the [back/cancel] button if the selected avatar was different to the active avatar, the active avatar is not changed.
- [ ] Pressing the [next button/confirm avatar] button if the selected avatar was different to the active avatar, the active avatar is changed to the selected avatar.

#### Unit 1.1.6: Persona information screen basic functionality.

**Priority**: Medium

**Test Description**:
Making sure that the UI is displayed correctly and that the user can enter persona information.

**Expected Output**:
- [ ] The user can see a text box for the name.
- [ ] The user can see a text box for the description.
- [ ] If the user tries to proceed without entering a name of at least 3 characters, the user is shown an error message.
- [ ] The user can see the icon of the current avatar on the right.
- [ ] The user can see the icon of the current avatar on the right blurred and scaled behind the avatar.
- [ ] When the user proceeds, the name and description are saved to the persona.
- [ ] When the user proceeds, the users avatar is set to the one displayed on the right.

#### Unit 1.1.7: Edit persona screen basic functionality.

**Priority**: Medium

**Test Description**:
Making sure that the user can successfully edit a persona.

**Glossary**
- Active Persona: The persona that the user has currently set to active in Emergence.
- Active Avatar: The avatar that the user has currently set to active in Emergence.
- Selected Avatar: The avatar that the user has selected to view in the edit persona screen, it may not be the same as the active avatar.

**Dependencies**:
- Unit 1.1.4, as you need to be able to create a persona to edit it.
- Unit 1.1.6, the basics of the form need to work.
- The user has at least two avatars.
- The users persona has a name and description.

**Required Inputs**:
- The user has at least two avatars.

**Expected Output**:
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
- [ ] If the persona is the active persona the user cannot see the delete button.

#### Unit 1.1.8: Edit/Create persona screen advanced functionality - correct flow.

**Priority**: Low

**Test Description**:
Testing that the edit/create persona screens are delivered in the correct order.

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

#### Unit 1.1.9: Create persona screen advanced functionality - starting fresh.

**Priority**: Low

**Test Description**:
Testing that the user can create a persona from scratch.

**Dependencies**:
- The user has no personas.

**Expected Output**:
- [ ] The user is taken to the create persona screen when pressing the central plus button.
- [ ] The user can complete the flow in 1.1.4.

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

## Featureset 3 Sample Project

### Feature 3.1 General Features

These are the features of the sample project (other than the interactable stations).

#### Unit 3.1.1: Open Overlay via keybind

**Priority**: High

**Test Description**:
Checking that the overlay can be opened via a keybind (Set in the Emergence prefab)

**Required Inputs**:
- Pressing the Z key (or whatever keybind is set in the Emergence prefab
- Use the PlayerPrefsEditor window to set the HasLoggedIn key to 0 (Window/Emergence Internal/PlayerPrefsEditor)

**Expected Output**:
- [ ] The Emergence overlay opens.
- [ ] The user is taken to the welcome screens.
- [ ] The user is taken to the login screen, and can complete the flow in 2.1.1.
- [ ] Reopen the overlay with the keybind and just see the login screen.

#### Unit 3.1.2: VRM Avatar Loading

**Priority**: High

**Test Description**:
Checking the player's character model is changed when a persona is loaded.

**Dependencies**:
- User is already signed into Emergence
- User has a persona
- User's persona has an EAS avatar

**Expected Output**:
- [ ] User's character model changes to be the avatar associated with the persona. (this may take up to 30s to change)

### Feature 3.2 Example Stations

These are the interactable stations, which each have button(s) to perform some action as part of an interactive example 

#### Unit 3.2.1: Basic functionality of the stations

**Priority**: High

**Test Description**:
Making sure that the UI is displayed correctly and that the user can interact with the stations.

**Expected Output**:
- [ ] The user can see the stations in the world.
- [ ] The user can see the buttons on the stations.
- [ ] The user can see the text on the stations.
- [ ] The user can see the text on the buttons when they are within a metre of the buttons.
- [ ] The user can no longer see the text when they move away from the buttons
- [ ] If the user is not signed in they see the text: Sign in using first station
- [ ] If the user is signed in they see the text: Press 'E' to activate
- [ ] If the user presses E something happens (see other tests)

#### Unit 3.2.2: Open Overlay Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by allowing the user to open the overlay.

**Dependencies**:
- 3.2.1 - Basic functionality of the stations

**Expected Output**:
- [ ] When the button is pressed, the overlay opens.

#### Unit 3.2.3: Request to Sign Message Station

**Priority**: High

**Test Description**:
Making sure the station works by sending a message to be signed to the users wallet.

**Dependencies**:
- 3.2.1 - Basic functionality of the stations

**Expected Output**:
- [ ] If the user presses the button a request to sign message should be sent to the user's wallet. 
- [ ] If the user signs, the signed message is shown to the user.
- [ ] If the user does not sign, the user is shown a message saying that an error occured during signing.

#### Unit 3.2.4: Reading and Writing To Smart Contracts Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by allowing the user to read and write from the simple smart contract.

**Dependencies**:
- 3.2.1 - Basic functionality of the stations

**Expected Output**:
- [ ] It shouldn't be possible to use both buttons at once.
- [ ] Pressing the GetCurrentCount button, it displays "Calling GetCurrentCount on 0xC9571AaF9EbCa8C08EC37D962310d0Ab9F8A5Dd2" to the user.
- [ ] Once the GetCurrentCount async request comes back from the ReadMethod, it outputs the number in the response to the user.
- [ ] Pressing displays "Calling GetCurrentCount on 0xC9571AaF9EbCa8C08EC37D962310d0Ab9F8A5Dd2." to the user. 
- [ ] If the user isn't on the Goerli network, they should recieve a message asking them to switch chain first, then the accept transaction message should show in their wallet.
- [ ] When the transaction has been confirmed, it displays a confirmation message to the user.
- [ ] After using the IncrementCount button, using the GetCurrentCount button again should give a higher number than it did previously.

#### Unit 3.2.5: Emergence Avatars Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by allowing the user to mint an EAS avatar.

**Dependencies**:
- 3.2.1 - Basic functionality of the stations
- The user does not already have Emergence Avatar A in their wallet

**Expected Output**:
- If the user presses the mint avatar button, to the user:
  - [ ] If the user isn't on the Polygon network, they should recieve a message asking them to switch chain first, then the accept transaction message should show in their wallet.
  - [ ] If the user is already on the Polygon network, the accept transaction message should show in their wallet without a switch chain message / confirmation.
- [ ] Once the transaction has been confirmed, the user should have a new avatar in their wallet.

#### Unit 3.2.6: Inventory Service Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by showing the user an example of an in-game inventory.

**Dependencies**:
- 3.2.1 - Basic functionality of the stations

**Expected Output**:
- [ ] If the user presses the in-game inventory button, it opens an example of an in-game inventory. Which should display NFTs from the logged-in users wallet.
- [ ] If the button is pressed after the inventory has been opened, then it closes the inventory.

#### Unit 3.2.7: Dynamic Metadata Station

**Priority**: High

**Test Description**:
Making sure the station works correctly by opening the NFT picker, and incrementing a counter in the dynamic metadata of the selected NFT.

**Dependencies**:
- 3.2.1 - Basic functionality of the stations

**Expected Output**:
- [ ] If the user presses the dynamic metadata button, it opens the collection screen with the default filter-set. 
- When the user selects an NFT with this picker: 
  - [ ] If there is already Dynamic Metadata it increments the dynamic metadata interger by 1 and then displays that on the right sidebar.
  - [ ] If there is no Dynamic Metadata it creates a new Dynamic Metadata with the value 1 and then displays that on the right sidebar.

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