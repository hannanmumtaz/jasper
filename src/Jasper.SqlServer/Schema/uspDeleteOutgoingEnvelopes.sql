CREATE PROCEDURE %SCHEMA%.uspDeleteOutgoingEnvelopes
    @IDLIST EnvelopeIdList READONLY
AS

    DELETE FROM %SCHEMA%.jasper_outgoing_envelopes WHERE id IN (SELECT ID FROM @IDLIST);
