CREATE TABLE machines(
    ip varchar(15) PRIMARY KEY NOT NULL,
    name varchar(50) NOT NULL,
    platform varchar(50) NOT NULL,
    leasedTo INTEGER
);

INSERT INTO machines VALUES ('192.168.1.1', 'HW1', 'XboxOne', NULL);
INSERT INTO machines VALUES ('192.168.1.2', 'HW2', 'PS4', NULL);
INSERT INTO machines VALUES ('192.168.1.3', 'HW3', 'XboxOne', NULL);
INSERT INTO machines VALUES ('192.168.1.4', 'HW4', 'PC', NULL);
INSERT INTO machines VALUES ('192.168.1.5', 'HW5', 'PC', NULL);