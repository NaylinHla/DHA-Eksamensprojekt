/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface AuthResponseDto {
  /** @minLength 1 */
  jwt: string;
}

export interface AuthLoginDto {
  /** @minLength 3 */
  email: string;
  /** @minLength 4 */
  password: string;
}

export interface AuthRegisterDto {
  /** @minLength 2 */
  firstName: string;
  /** @minLength 2 */
  lastName: string;
  /** @minLength 3 */
  email: string;
  /**
   * @format date-time
   * @minLength 1
   */
  birthday: string;
  /** @minLength 1 */
  country: string;
  /** @minLength 4 */
  password: string;
}

export interface GetAllSensorHistoryByDeviceIdDto {
  /** @format guid */
  deviceId?: string;
  deviceName?: string;
  sensorHistoryRecords?: SensorHistoryDto[];
}

export interface SensorHistoryDto {
  /** @format double */
  temperature?: number;
  /** @format double */
  humidity?: number;
  /** @format double */
  airPressure?: number;
  /** @format int32 */
  airQuality?: number;
  /** @format date-time */
  time?: string;
}

export interface AdminChangesPreferencesDto {
  deviceId?: string;
  unit?: string;
  interval?: string;
}

export interface ChangeSubscriptionDto {
  clientId?: string;
  topicIds?: string[];
}

export interface ExampleBroadcastDto {
  eventType?: string;
  message?: string;
}

export type AdminHasDeletedData = ApplicationBaseDto & {
  eventType?: string;
};

export interface ApplicationBaseDto {
  eventType?: string;
}

export type ServerBroadcastsLiveDataToDashboard = ApplicationBaseDto & {
  logs?: GetAllSensorHistoryByDeviceIdDto[];
  eventType?: string;
};

export type MemberLeftNotification = BaseDto & {
  clientId?: string;
  topic?: string;
};

export interface BaseDto {
  eventType?: string;
  requestId?: string;
}

export type ExampleClientDto = BaseDto & {
  somethingTheClientSends?: string;
};

export type ExampleServerResponse = BaseDto & {
  somethingTheServerSends?: string;
};

export type Ping = BaseDto & object;

export type Pong = BaseDto & object;

export type ServerSendsErrorMessage = BaseDto & {
  message?: string;
};

/** Available eventType and string constants */
export enum StringConstants {
  AdminHasDeletedData = "AdminHasDeletedData",
  ServerBroadcastsLiveDataToDashboard = "ServerBroadcastsLiveDataToDashboard",
  MemberLeftNotification = "MemberLeftNotification",
  ExampleClientDto = "ExampleClientDto",
  ExampleServerResponse = "ExampleServerResponse",
  Ping = "Ping",
  Pong = "Pong",
  ServerSendsErrorMessage = "ServerSendsErrorMessage",
  Dashboard = "Dashboard",
  Device = "Device",
  SensorData = "SensorData",
  ChangePreferences = "ChangePreferences",
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<FullRequestParams, "body" | "method" | "query" | "path">;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (securityData: SecurityDataType | null) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown> extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5000";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) => fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter((key) => "undefined" !== typeof query[key]);
    return keys
      .map((key) => (Array.isArray(query[key]) ? this.addArrayQueryParam(query, key) : this.addQueryParam(query, key)))
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string") ? JSON.stringify(input) : input,
    [ContentType.Text]: (input: any) => (input !== null && typeof input !== "string" ? JSON.stringify(input) : input),
    [ContentType.FormData]: (input: FormData) =>
      (Array.from(input.keys()) || []).reduce((formData, key) => {
        const property = input.get(key);
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData()),
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(params1: RequestParams, params2?: RequestParams): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (cancelToken: CancelToken): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(`${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`, {
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type && type !== ContentType.FormData ? { "Content-Type": type } : {}),
      },
      signal: (cancelToken ? this.createAbortSignal(cancelToken) : requestParams.signal) || null,
      body: typeof body === "undefined" || body === null ? null : payloadFormatter(body),
    }).then(async (response) => {
      const r = response.clone() as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const data = !responseFormat
        ? r
        : await response[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://localhost:5000
 */
export class Api<SecurityDataType extends unknown> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Auth
     * @name AuthLogin
     * @request POST:/api/auth/Login
     */
    authLogin: (data: AuthLoginDto, params: RequestParams = {}) =>
      this.request<AuthResponseDto, any>({
        path: `/api/auth/Login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthRegister
     * @request POST:/api/auth/Register
     */
    authRegister: (data: AuthRegisterDto, params: RequestParams = {}) =>
      this.request<AuthResponseDto, any>({
        path: `/api/auth/Register`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name AuthSecured
     * @request GET:/api/auth/Secured
     */
    authSecured: (params: RequestParams = {}) =>
      this.request<File, any>({
        path: `/api/auth/Secured`,
        method: "GET",
        ...params,
      }),
  };
  getSensorDataByDeviceId = {
    /**
     * No description
     *
     * @tags GreenhouseDevice
     * @name GreenhouseDeviceGetSensorDataByDeviceId
     * @request GET:/GetSensorDataByDeviceId
     */
    greenhouseDeviceGetSensorDataByDeviceId: (
      query?: {
        /** @format guid */
        deviceId?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<GetAllSensorHistoryByDeviceIdDto[], any>({
        path: `/GetSensorDataByDeviceId`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  adminChangesPreferences = {
    /**
     * No description
     *
     * @tags GreenhouseDevice
     * @name GreenhouseDeviceAdminChangesPreferences
     * @request POST:/AdminChangesPreferences
     */
    greenhouseDeviceAdminChangesPreferences: (data: AdminChangesPreferencesDto, params: RequestParams = {}) =>
      this.request<File, any>({
        path: `/AdminChangesPreferences`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
  deleteData = {
    /**
     * No description
     *
     * @tags GreenhouseDevice
     * @name GreenhouseDeviceDeleteData
     * @request DELETE:/DeleteData
     */
    greenhouseDeviceDeleteData: (params: RequestParams = {}) =>
      this.request<File, any>({
        path: `/DeleteData`,
        method: "DELETE",
        ...params,
      }),
  };
  subscribe = {
    /**
     * No description
     *
     * @tags Subscription
     * @name SubscriptionSubscribe
     * @request POST:/Subscribe
     */
    subscriptionSubscribe: (data: ChangeSubscriptionDto, params: RequestParams = {}) =>
      this.request<File, any>({
        path: `/Subscribe`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
  unsubscribe = {
    /**
     * No description
     *
     * @tags Subscription
     * @name SubscriptionUnsubscribe
     * @request POST:/Unsubscribe
     */
    subscriptionUnsubscribe: (data: ChangeSubscriptionDto, params: RequestParams = {}) =>
      this.request<File, any>({
        path: `/Unsubscribe`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
  exampleBroadcast = {
    /**
     * No description
     *
     * @tags Subscription
     * @name SubscriptionExampleBroadcast
     * @request POST:/ExampleBroadcast
     */
    subscriptionExampleBroadcast: (data: ExampleBroadcastDto, params: RequestParams = {}) =>
      this.request<File, any>({
        path: `/ExampleBroadcast`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
}
