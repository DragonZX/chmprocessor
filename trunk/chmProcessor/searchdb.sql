
CREATE TABLE Word ( 
	WrdCod INTEGER PRIMARY KEY ,
	WrdTxt TEXT
);

CREATE UNIQUE INDEX WrdText ON Word ( 
	WrdTxt
);

CREATE TABLE Document (
    DocCod INTEGER PRIMARY KEY,
    DocPat TEXT,
    DocDes TEXT,
    DocLen INTEGER
);

CREATE TABLE WordInstance (
	InsWrdCod INTEGER,
	InsDocCod TEXT,
	InsCount INTEGER ,
	InsPos INTEGER,
	
	PRIMARY KEY ( InsWrdCod , InsDocCod )
);

CREATE UNIQUE INDEX InsDocument ON WordInstance ( 
	InsDocCod , InsWrdCod 
);

CREATE TABLE IndexCfg (
	CfgCod INTEGER PRIMARY KEY,
	CfgLanguage TEXT
);
