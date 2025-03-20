-- Users Table
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Bio TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
-- Skills Table
CREATE TABLE Skills (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) UNIQUE NOT NULL
);
-- UserSkills Junction Table
CREATE TABLE UserSkills (
    UserId INT REFERENCES Users(Id) ON DELETE CASCADE,
    SkillId INT REFERENCES Skills(Id) ON DELETE CASCADE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, SkillId)
);

-- Connections Table
CREATE TABLE Connections (
    UserId INT REFERENCES Users(Id) ON DELETE CASCADE,
    ConnectedUserId INT REFERENCES Users(Id) ON DELETE CASCADE,
    Status VARCHAR(20) CHECK (Status IN ('Pending', 'Accepted', 'Canceled')) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, ConnectedUserId),
    CONSTRAINT NoSelfConnection CHECK (UserId <> ConnectedUserId)
);

-- Prevent duplicate A→B and B→A connections
CREATE UNIQUE INDEX IdxUniqueConnections ON Connections (
    LEAST(UserId, ConnectedUserId),
    GREATEST(UserId, ConnectedUserId)
);

-- Posts Table
CREATE TABLE Posts (
    Id SERIAL PRIMARY KEY,
    UserId INT REFERENCES Users(Id) ON DELETE CASCADE,
    Content TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- MODIFIED: Messages Table (now supports rooms)
CREATE TABLE Messages (
    Id SERIAL PRIMARY KEY,
    SenderId INT REFERENCES Users(Id) ON DELETE CASCADE,
    ReceiverId INT REFERENCES Users(Id) ON DELETE CASCADE, -- For direct messages
    RoomId INT REFERENCES Rooms(Id) ON DELETE CASCADE,     -- For group messages -- NEW
    Content TEXT NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    -- Ensure message is either direct (SenderId + ReceiverId) or in a room (RoomId) -- NEW
    CONSTRAINT CheckMessageTarget CHECK (
        (RoomId IS NOT NULL AND ReceiverId IS NULL) OR
        (RoomId IS NULL AND SenderId IS NOT NULL AND ReceiverId IS NOT NULL)
    )
);

-- NEW: Rooms Table
CREATE TABLE Rooms (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CreatorId INT NOT NULL default 1 REFERENCES Users(Id) ON DELETE CASCADE;
);

-- NEW: RoomUsers Junction Table
CREATE TABLE RoomUsers (
    RoomId INT REFERENCES Rooms(Id) ON DELETE CASCADE,
    UserId INT REFERENCES Users(Id) ON DELETE CASCADE,
    JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (RoomId, UserId)
);



CREATE TABLE Invitations (
    Id SERIAL PRIMARY KEY,
    InviterId INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    InviteeId INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    RoomId INT NOT NULL REFERENCES Rooms(Id) ON DELETE CASCADE,
    Status VARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Accepted', 'Declined', 'Expired')) DEFAULT 'Pending',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (InviterId, InviteeId, RoomId), -- Prevent duplicate invitations
    CHECK (InviterId <> InviteeId) -- No self-invitations
);

-- Indexes for performance
CREATE INDEX IdxInvitationsInviteeId ON Invitations(InviteeId);
CREATE INDEX IdxInvitationsStatus ON Invitations(Status);
CREATE INDEX IdxUserSkillsUserId ON UserSkills(UserId);
CREATE INDEX IdxConnectionsUserId ON Connections(UserId);
CREATE INDEX IdxPostsUserId ON Posts(UserId);
CREATE INDEX IdxMessagesReceiverId ON Messages(ReceiverId);
CREATE INDEX IdxMessagesRoomId ON Messages(RoomId);       
CREATE INDEX IdxRoomUsersRoomId ON RoomUsers(RoomId);    
CREATE INDEX IdxRoomUsersUserId ON RoomUsers(UserId);    