// src/app/core/models/api-error.ts
//
// ProblemDetails interface (RFC 7807) and ApiError class.
//
// Used by errorInterceptor to normalise all HTTP errors into a consistent shape
// before they reach component-level error handlers. By standardising on RFC 7807,
// all components and global error handlers can use a single error type rather than
// checking both string errors and HttpErrorResponse objects.
//
// The ApiError class extends Error so it satisfies the Angular ErrorHandler contract
// (which receives unknown errors) while carrying the structured ProblemDetails payload.
// This satisfies the Liskov Substitution Principle — any code expecting an Error works
// correctly with ApiError.

/**
 * RFC 7807 Problem Details structure returned by all Traverse microservices.
 * The `errors` map carries field-level validation errors for form binding.
 */
export interface ProblemDetails {
  /** A URI reference that identifies the problem type. */
  type: string;
  /** A short, human-readable summary of the problem type. */
  title: string;
  /** The HTTP status code. */
  status: number;
  /** A human-readable explanation specific to this occurrence. */
  detail: string;
  /** A URI reference that identifies the specific occurrence (correlation ID / trace). */
  instance?: string;
  /** Validation field errors — key: field name, value: array of error messages. */
  errors?: Record<string, string[]>;
}

/**
 * ApiError wraps a ProblemDetails payload as a standard JavaScript Error.
 *
 * Why extend Error: Angular's ErrorHandler receives `unknown`. Extending Error
 * allows callers to use `instanceof ApiError` narrowing without breaking the
 * Error type contract. The message property is set to problem.detail so that
 * standard Error logging tools display a useful message.
 */
export class ApiError extends Error {
  constructor(public readonly problem: ProblemDetails) {
    super(problem.detail ?? problem.title);
    this.name = 'ApiError';

    // Restore prototype chain — required when extending built-in classes in
    // environments targeting ES5 or ES2015 with downlevel compilation.
    Object.setPrototypeOf(this, ApiError.prototype);
  }
}
