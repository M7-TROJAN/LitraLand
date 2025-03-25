--CREATE VIEW MostPopularBooksView AS
--SELECT 
--    bc.BookId,
--    b.Title,
--    b.ImageThumbnailUrl,
--    a.Name AS AuthorName,
--    COUNT(*) AS RentalCount
--FROM RentalCopies rc
--JOIN BookCopies bc ON rc.BookCopyId = bc.Id
--JOIN Books b ON bc.BookId = b.Id
--JOIN Authors a ON b.AuthorId = a.Id
--WHERE b.IsDeleted = 0
--GROUP BY bc.BookId, b.Title, b.ImageThumbnailUrl, a.Name

--SELECT * FROM MostPopularBooksView
--ORDER BY RentalCount DESC;


--drop view MostPopularBooksView;




CREATE PROCEDURE GetMostPopularBooks
    @TopN INT = NULL -- يمكن أن يكون NULL افتراضيًا
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (CASE WHEN @TopN IS NULL THEN 1000000 ELSE @TopN END) -- إذا كانت NULL، رجّع كل الكتب
        bc.BookId,
        b.Title,
        b.ImageThumbnailUrl,
        a.Name AS AuthorName,
        COUNT(*) AS RentalCount
    FROM RentalCopies rc
    JOIN BookCopies bc ON rc.BookCopyId = bc.Id
    JOIN Books b ON bc.BookId = b.Id
    JOIN Authors a ON b.AuthorId = a.Id
    WHERE b.IsDeleted = 0
    GROUP BY bc.BookId, b.Title, b.ImageThumbnailUrl, a.Name
    ORDER BY RentalCount DESC;
END;


EXEC GetMostPopularBooks @TopN = 5;

EXEC GetMostPopularBooks @TopN = NULL;

EXEC GetMostPopularBooks;