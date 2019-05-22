namespace min {
public abstract class Expression
{
public abstract T Accept<T>(IVisitor<T> visitor);

public interface IVisitor<T>
{
T VisitBinary (Binary expression);
T VisitGrouping (Grouping expression);
T VisitLiteral (Literal expression);
T VisitUnary (Unary expression);
}

public class Binary : Expression
{
public readonly Expression left;
public readonly Token @operator;
public readonly Expression right;
public Binary (Expression left, Token @operator, Expression right)
{
this.left = left;
this.@operator = @operator;
this.right = right;
}

public override T Accept<T>(IVisitor<T> visitor)
{
return visitor.VisitBinary(this);
}
}

public class Grouping : Expression
{
public readonly Expression expression;
public Grouping (Expression expression)
{
this.expression = expression;
}

public override T Accept<T>(IVisitor<T> visitor)
{
return visitor.VisitGrouping(this);
}
}

public class Literal : Expression
{
public readonly object value;
public Literal (object value)
{
this.value = value;
}

public override T Accept<T>(IVisitor<T> visitor)
{
return visitor.VisitLiteral(this);
}
}

public class Unary : Expression
{
public readonly Token @operator;
public readonly Expression right;
public Unary (Token @operator, Expression right)
{
this.@operator = @operator;
this.right = right;
}

public override T Accept<T>(IVisitor<T> visitor)
{
return visitor.VisitUnary(this);
}
}
}
}
