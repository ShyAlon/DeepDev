var englishTranslation = {
    auditTrail: "<h1>Audit</h1>The audit of the current entity - its creation and last modification.",
    sessionTrail: "<h1>Session</h1>The currency of the active session.",
	saveButton: "Save '{{data.Entity.Name}}'.",
	taskButton: "Add a task related to '{{data.Entity.Name}}'.",
	noteButton: "Add a free text note related to '{{data.Entity.Name}}'.",
	deleteButton: "<h1>Careful</h1>Delete '{{data.Entity.Name}}' and all its heirarchy.",
    createProjectButton: "Create a new project",
	childButton: "Add a {{property | typeToSingle}} to '{{data.Entity.Name}}'.",
	duplicateButton: "Create a deep copy of '{{data.Entity.Name}}'.",
    sequenceEdit: "You can add a selected method (from the selected component in the selected method), add a method return or a no-return marker.",
    sequenceMethod: "boom boom",
    noTasksHeader: "Hurray, no tasks!",
    noTasksText: "So, as you can see the projects in the left hand side of the screen have no tasks assigned to you. Maybe it's time for a new project?",
	
	/* Item Properties */
	itemName: "The name of '{{data.Entity.Name}}'.",
	itemDescription: "The textual description of '{{data.Entity.Name}}'.",
	
	itemOwner: "The person responsible for {{data.Entity.Name}}",
	itemDeadline: "The due date of {{data.Entity.Name}}",
	itemProcess: "The process hosting {{data.Entity.Name}}",
	itemStatus: "The execution reported status of the {{data.Type | typeToSingle}} {{data.Entity.Name}}",
	itemPriority: "The importance of {{data.Entity.Name}} with relation to other {{data.Type | typeToSingle}}s",
	itemEffort: "The effort put into the {{data.Type | typeToSingle}} {{data.Entity.Name}}",
	itemEffortEstimation: "The effort estimation attached to the {{data.Type | ordinal}} {{data.Entity.Name}} before implementation",
	itemRiskType: "The risk attached to the {{data.Type | typeToSingle}} {{data.Entity.Name}}",
	itemRiskStatus: "The resolution level of the risk attached to the {{data.Type | typeToSingle}} {{data.Entity.Name}}",
	itemNewRequirement: "A requirement to attach to the {{data.Type | typeToSingle}} {{data.Entity.Name}}",
	itemPredecessorIds: "A sibling {{data.Type | typeToSingle}} which must precede {{data.Entity.Name}}",
	itemContainerID: "The sprint or milestone the {{data.Type | typeToSingle}} will be included in.",
	itemProjectUser: "Assign a role (system's engineer, product manager or project manager) to a user in you organization.",
	itemResult: "The expected outcome of the use case.",
	itemInitiator: "The element or actor which starts the use case.",
	itemImage: "Drop and image and save the note for the image to be displayed in the relevant documents.",
	itemIndex: "The index of the entity. Entities of the same parent are displayed sorted by their index.",
	itemStartTime: "The time {{data.Entity.Name}} is planned to start.",
	itemEndTime: "The time {{data.Entity.Name}} is planned to end.",
	itemEffortEstimationOptimistic: "The minimum possible time required to accomplish {{data.Entity.Name}}, assuming everything proceeds better than is normally expected",
	itemEffortEstimationPessimistic: "The maximum possible time required to accomplish {{data.Entity.Name}}, assuming everything goes wrong (but excluding major catastrophes).",
	itemEffortEstimationMean: "The best estimate of the time required to accomplish {{data.Entity.Name}}, assuming everything proceeds as normal.",

	methodModule: "The module providing the required functionality",
	methodComponent: "The component within the chosen module providing the required functionality",
	methodMethod: "The method of the chosen component providing the required functionality",

	sequenceAddMethod: "<h1>Add Method</h1>Add the selected method at the end of the sequence.",
	sequenceAddReturn: "<h1>Add Return</h1>Add a return marker to the end of the sequence returning from the last method.",
	sequenceAddNoReturn: "<h1>Add No Return</h1>Add a no return marker to the end of the sequence returning to the method before last.",

    /* Block */
	itemSystemBlock: "<h1>{{data.Name}}</h1><h4>{{property.slice(0,-1)}}</h4>{{data.Description}}",
	itemMissingMethods: "The methods defined in the related sequences which are not yet defined in {{data.Type | typeToSingle}} {{data.Entity.Name}}",
	attachedItem: "{{data.Name}} is a {{property | typeToSingle}} related to {{main.Entity.Name}}",
	sequenceMethod: "<h1>{{data.Entity.Name}}</h1><h4>Module: {{data.Module}}</h4><h4>Component: {{data.Parent.Name}}</h4><h5>Risk: {{data.Entity.RiskType}}</h5>{{data.Entity.Description}}",
	methodNoReturnMarker: "A marker for continuing the sequence with a synchronous return",
	methodReturnMarker: "A marker for continuing the sequence after terminating the function call sequemce",

	auditBlock: "<h1>Audit</h1>The audit trail for <i>{{data.Entity.Name}}</i>",
	sessionBlock: "<h1>Session</h1>The session information for the current session",
	tagsBlock: "<h1>Tags</h1>Add tags to the current {{data.Type}} to include it in groups and search patterns.",
	navigationBlock: "<h1>Project tree</h1>The hierarchical tree structure representing the aggregative relations between the project entities",

    /* Entity Descriptions */
	NoteDescription: "<h1>Notes</h1>Notes are free text entities used for unstructured description of entities and <b>should</b> be converted to structured entities (such as tasks or issues). Notes can include images which are displayed below the note.",
	ProjectDescription: "<h1>Project</h1>A Project is a temporary endeavor undertaken to create a unique product, service, or result (PMBoK). Projects are the top level entities in DeepDev and represent the entire life cycle involved with the described product or service.<br/>Projects have aspects of requirements which are managed under milestones and deliverables which are managed  under modules.<br/>The recommended flow would usually be<ul><li>Create requirements, use cases and sequences</li><li>Edit, modify and enhance the resulting modules and components</li><li>Follow through with tasks and issues</li></ul>",
	RequirementDescription: "<h1>Requirement</h1>A descriptor of the system which presents a functional capability which needs to be met or a non-functional constraint which needs to be taken into account.",
	MilestoneDescription: "<h1>Milestone</h1>A milestone is a significant point or event in a project. Milestones represent the fulfillment of requirements which (after implemented) reduces the risk involved with the project significantly.",
	TaskDescription: "<h1>Task</h1>A Task is an actionable item which needs to be performed in the scope of the project.",
	UseCaseDescription: "<h1>Use Case</h1>A Use Case is a description of a certain, specific usage which need to be supported successfully by the project's result.",
	SequenceDescription: "<h1>Sequence</h1>A Sequence is a technical description of a series of actions which need to be supported successfully by the project's components and provided as methods.",
	IssueDescription: "<h1>Issue</h1>An Issue represents a concern, a difficulty or an adjustment related to a containing task.",
	ModuleDescription: "<h1>Module</h1>A Module is a group of components with shared functionality, objectives and interfaces.",
	ComponentDescription: "<h1>Component</h1>A Component is an implementation unit with a functionality, objectives and interface as exposed by its methods.",
	MethodDescription: "<h1>Method</h1>A Method is an API exposed by a component.",

    /* General descriptions*/
	DeepDevDescription: "DeepDev is a project engineering management system and provides an intuitively simple interface for creating projects, managing the design process and following the implementation progress with regard to risk, requirements’ compliance and other critical factors.",
	DeepDevProcess: "The life cycle of a project in DeepDev is:",
	DeepDevProcess2: "Systems engineer creates technical sequences for the use cases and translates the requirements to system modules and components.",
	DeepDevProcess3: "Project manager includes tasks in milestones and sprints and assigns the tasks to team members.",
	DeepDevProcess4: "Project is monitored by all roles for until completed successfully.",
	DeepDevProcess1: "The Product manager defines the project's requirements and breaks them down into use cases.",
	InitialiInstructions: "Press 'F1' to take a tour of the page you are in, 'F2' to edit some document attributes (when showing a document) and CTRL-S to save.",

	DeepDevProduct: "Defining a product in DeepDev involves creating a set of requirements:",
	DeepDevProduct1: "Include non-functional requirements like resource limitation, operating system or mandatory infrastructure.",
	DeepDevProduct2: "Include functional requirements to describe the required behavior of the product.",
	DeepDevProduct3: "For functional requirements add use cases to further detail the scenarios for each requirement.",
	DeepDevProduct4: "Requirements and use cases are found under the 'Requirements' branch of the project.",

	DeepDevDesign: "Once the requirements are formulated the system design can be generated:",
	DeepDevDesign1: "Add sequences for each use case and include initiali technical data.",
	DeepDevDesign2: "Edit and modify the resulting modules. Assign those modules to processes.",
	DeepDevDesign3: "Edit and modify the components in the resulting modules.",
	DeepDevDesign4: "Edit and modify the methods of the resulting modules. Remember that this is the public API of the components.",
	DeepDevDesign5: "Modules and components are found under the 'Modules' branch of the project.",
	

    /* Sentences */
    createProject: "Create a New Project",

    /* The Process*/


    /* Account */


    userEMail: "The email address used to uniquely identify the user",
    userToken: "The token verifying you are authorized to edit the user credentials",
    userPassword: "The user's password",
    userName: "The name of the user",
    userOrganization: "The unique identifier of the organization the user belongs to. It is used for sharing projects inside an organization."
};
